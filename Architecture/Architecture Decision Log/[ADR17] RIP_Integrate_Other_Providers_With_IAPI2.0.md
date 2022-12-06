# IAPI 2.0 Integration in Other Providers

## Status

Proposed

## Context

The biggest trouble we currently have with Other Providers in R1 is job failing on Kubernetes due to containers crash or lost connectivity to services. We're unable to recover from such failures and job is temporarily left in unknown status, then after certain period of time (30 minutes currently) marked as failed and user must manually run the job again. Second problem in my opinion is how Other Providers are designed - RIP creates child jobs for each 1000 items. So if user tries to run job for 1M records, we create 1k child jobs and each of those jobs has to be picked up and processed by the agent separately. This approach is problematic because of several reasons:

- it adds unncecessary complexity and is error prone
- it's hard to track overall job progress (JobTracker tables were created for this purpose)
- job is progressing slower (overhead is added for picking up the job by the agent, setting up each of the small jobs etc.)
- it's very hard to make any changes to the code because of how it was designed (or rather lack of design)

## Solution

There is only one good solution - rewrite "Other Providers" code using IAPI 2.0. This has huge advantages:

- we will have full control over the load file building (until IAPI 2.0 implements streaming) so it will be container crash-resistant and drain stop will work as it should
- actual import of the load file to the workspace is outside of our responsibility and we don't have to worry about it
- whole process will be packed in one single RIP job (we could finally completely get rid of problematic job tracker tables, SyncWorker, SyncManager, RdoSynchronizer and many other black magic classes)
- cleaner code and architecture so everyone will know how it works

There are some caveats however:

- we won't be able to rewrite and run the code on production chunk-by-chunk (only all at once when it's done)
- no one fully understands current code, so it may be hard to re-implement all the functionalities as they work right now, without introducing defects or breaking changes

## Design

We want to do all the processing during single RIP job. First of all, we must have a toggle to enable/disable new flow. Toggle should be checked in `TaskFactory.CreateTask` and based on its value we return either existing `SyncManager` (when toggle is disabled - by default) or class implementing new logic (if the toggle is enabled). This class must implement `ITask` interface and we can name it `CustomProviderTask` for example.

High level overview of how custom providers work - we cannot introduce any breaking changes here as it will affect existing LDAP, FTP, AzureAD and possibly other providers:

- they implement `IDataSourceProvider` interface:

```csharp
  public interface IFieldProvider
  {
    IEnumerable<FieldEntry> GetFields(
      DataSourceProviderConfiguration providerConfiguration);
  }

  public interface IDataSourceProvider : IFieldProvider
  {
    IDataReader GetData(
      IEnumerable<FieldEntry> fields,
      IEnumerable<string> entryIds,
      DataSourceProviderConfiguration providerConfiguration);

    IDataReader GetBatchableIds(
      FieldEntry identifier,
      DataSourceProviderConfiguration providerConfiguration);
  }
```

- `GetFields` method is called from Web in order to get mappable fields (`FtpProviderAPIController` for FTP and `SourceFieldsController` for the rest of custom providers) so it's not relevant for us
- we will still have to get list of IDs in the first step (`GetBatchableIds`), and store them somewhere (details below)
- then we take portion of the IDs (in batches, for example 10k each) and call `GetData` passing in those IDs
- in the return value we get `IDataReader` which we will use to read all the items and store them in a load file
- once we finish building all the load files, we create new Import API 2.0 job and feed it with the load files we built
- RIP agent must stay alive until Import API 2.0 finish its work
- when IAPI job completes, we get and report item level errors
- e-mail notification should be send if needed

**Important things to keep in mind during the implementation:**

- we must not try to understand old code, and definitely not copy and paste it. We should forget about it, and implement the solution from the ground up, knowing what is the goal and how it should work at the end
- remember that the K8s container can crash at anytime, so be prepared

## Load File building

### ID Files

First stage is to store list of IDs in a file. This can be simple text file, where each line contains record identifier. Also, reasonable approach would be to build load files in batches, for example 10k records each because of performance reasons. So for each batch we would have one file with IDs and one file with records, like below:

```
9ecf9b4e-50b9-4cba-980f-3295a8c6363a.ID.001
9ecf9b4e-50b9-4cba-980f-3295a8c6363a.DATA.001
9ecf9b4e-50b9-4cba-980f-3295a8c6363a.ID.002
9ecf9b4e-50b9-4cba-980f-3295a8c6363a.DATA.002
```

GUID above can be a `BatchInstance` from `JobDetails` but that's only proposal. Anyway, names should be predictable so we can easily tell to which job they belong.

File can simply contain identifiers, each in separate line:

```
ITEM_0001
ITEM_0002
...
```

While creating a file containing IDs, we must do it in "all or nothing" fashion, meaning if the container crashes in the middle, we start the process over again. In order to check if all ID files are created, we should store some information for example in `JobDetails`. It can be simple flag e.g. `AreIDsReady`. We set it to `true` only when we successfully create all ID files. When container crashes and next agent picks up the job, it should check this flag, and if it's `false`, we retrieve IDs from the Custom Provider once again, and overwrite ID files. When the process completes, we set the flag to `true` so we can skip this step if the container crashes again.

Now we have ID files ready and can proceed to actual load file building.

### Data Files

Next we want to take each of the ID files (sequentially), read the IDs and use them to call `IDataSourceProvider.GetData`. In return we get `IDataReader` from which we get records for requested identifiers. This part is more tricky as it must be drain-stoppable and container crash proof. We read records from `IDataReader` one by one and write data to load file line by line. We must store number of written records, for example in `JobDetails`. When drain-stop occurrs, we stop reading from `IDataReader` and store the number of processed items correlated with batch index. The value of `JobDetails` can be JSON:

```json
{
    "BatchInstance": "9ecf9b4e-50b9-4cba-980f-3295a8c6363a",
    "AreIDsReady": true,
    "Batches": [
        {
            "BatchID": 0,
            "NumberOfRecordsProcessed": 666,
            "IsCompleted": false
        },
        {
            "BatchID": 1,
            "NumberOfRecordsProcessed": 0,
            "IsCompleted": false
        }
    ]
}
```

On resume, we read `NumberOfRecordsProcessed` from `JobDetails`, read ID file contents skipping number of lines equal to `NumberOfRecordsProcessed` and call `IDataSourceProvider.GetData` for the rest of them. When we finish writing of load file, we set `IsCompleted = true` so we can skip it next time.

Remaining question is, what if container crashes. We have two options:

1. Store number of processed records in `JobDetails` after each record (a lot of SQL updates, performance will suffer)
1. Store number of processed records in `JobDetails` only after load file building is complete (preferable solution)

Second option is better I think. If the container crashes, how do we know how many records were already processed? We can read the load file and count line numbers, and that's it. I don't think performance of reading the file will have big impact, because we know that container crashes are not happening so often, and most of the jobs complete successfully today, so it's edge case anyway.

### Cleanup

After job completes, we must clean up load files and files containing IDs.

## Import API 2.0 job

We already have experience with IAPI 2.0 from Relativity.Sync so there should be no surprise. We create and configure the job, add load files as data source, run the job and wait for results. When job is complete, we must gather and report item level errors.

## Entity import

This is most problematic part because of Managers linking process. I would implement it as separate service because it requires its own logic regarding fields mapping and Managers linking. There are couple of things to keep in mind:

- require `First Name` and `Last Name` fields in the mapping
- dynamically build `Full Name` and include it in the load file (remember to give it unique name to avoid conflicts with user-mapped fields, e.g. `FullName-1d6f71c8-05d5-4af0-9752-09ef2a7a627c`)
- handle Manager Field ID

Consider using ObjectManager mass update to link managers, or use Import API 2.0 as usual. Entity import is a little bit tricky and might require its own spike.

## What's next?

Next, we would love to clean up all the legacy mess. When implementation is done and proven to work on production, we would like to get rid of following classes, ideally along with their unused dependencies:

- `SyncManager`
- `SyncWorker`
- `RdoEntitySynchronizer`
- `SyncEntityManagerWorker`
- unfortunately we have to leave `RdoSynchronizer` alone, because it's used in productions push and to get list of fields in workspace. We can only remove method `public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options, IJobStopManager jobStopManager, IDiagnosticLog diagnosticLog)` along with its unused dependencies. The other (overloaded) `SyncData` method is used in production push so it needs to stay untouched. Optionally, we could think about extracting `GetFields` method to separate service.
