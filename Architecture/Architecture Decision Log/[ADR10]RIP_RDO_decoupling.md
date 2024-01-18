# Decouple Sync from RIP RDOs

## Status

Proposed

## Context

At the moment, Sync is tightly coupled with RIP RDOs. This unables Sync to function without RIP application being installed into workspace.

The most difficult part of decoupling, is not to break existing customer RIP workflow. Reporting item and job level errors, updating number of records transferred and failed, and documents tagging - all those features **cannot change** and still must be working as designed.

List of RIP RDOs, which Sync depends on:

- `Job History` - it's the parent object of `Sync Configuration` object type. It is widely used by Sync to: validate job name, tag documents in source and destination workspace (by combining job name and Job History Artifact ID), update job progress (number of items transferred and failed), create item level errors and job level errors, and finally - job retries.
- `Job History Error` - represents the error entry - either item or job level error. Its' parent object is `Job History`.
- `Destination Workspace` - required to tag documents in source workspace.
- `Relativity Source Case` and `Relativity Source Job` - required to tag documents in destination workspace. Those two object types are **already fully decoupled** from RIP, which means that Sync doesn't require RIP to be installed anywhere to tag documents in destination. Sync is able to dynamically create them on demand (`DestinationWorkspaceObjectTypesCreationExecutor`).

## Assumptions

- Sync require `Sync Configuration` instance to be created prior to the job, which implicates...
- ...that external application must know all Sync's RDOs, and must be able to create them in the workspace when installed (Application Schema)

## Option 1

### **Reporting job errors and updating job progress**

There are at least two possible ways:

1. Extend `SyncJob` to pass job history progress and errors handlers

Currently, Sync is reporting job progress and creating job errors entries by itself via ObjectManager - it must know RIP `Job History` and `Job History Error` RDOs. Instead, we can pass to `SyncJob` interfaces which external application must implement to provide desired functionality. That way, the caller will be responsible for handling and displaying progress and errors.

- Updating job progress - we can extend existing `IProgress<SyncJobState>` interface, which is currently used to update job state. We could add new properties: `TotalItemsCount`, `TransferredRecordsCount` and `FailedItemsCount` which external app can use to update its' job history object accordingly.
- Reporting item level errors and job level errors - we can create new interface similar to `IProgress<T>`, which will be implemented by the external application. That way, the external app will be responsible for creating its own job error entries. Example interface:

```cs
public interface IJobErrorHandler
{
    Task CreateItemLevelErrorAsync(string sourceUniqueId, string errorMessage, string stackTrace);
    Task CreateJobLevelErrorAsync(string sourceUniqueId, string errorMessage, string stackTrace);
}
```

2. Create new RDOs in Sync

Another option is to create new RDOs in Sync: `Sync Job History` and `Sync Job History Error`. That way Sync will report progress and errors in its own RDOs, so disadvantage of such approach is that we would have to create adapter in RIP, which will translate Sync's job history and error entries into existing RIP `Job History` and `Job History Error` RDOs, resulting in duplicated data. But on the other hand, all the job history and job history erors would be fully controlled by Sync, so it will solve another problems which are described below.

### **Job retries**

This functionality has hard dependency on RIP's `Job History` RDO. We are using `Job History` object name and Artifact ID to build a query for Export API. To be fully decoupled from RIP's `Job History` we have two options:

1. Pass built query or query results to Sync - external app would be responsible for the logic to collect document list to retry (this is probably a bad idea)
2. Create `Sync Job History` and `Sync Job History Error` RDOs in Sync - that way, we would have full control over storing and reading Sync's job history and job history errors.

### **Validating job name**

As mentioned previously, right now Sync is querying for Job History name by Artifact ID so it has to know RIP's `Job History` GUID. Again, two possibilities here:

1. Create `Job History Name` field in `Sync Configuration` so the external app would be responsible for passing into configuration job history name.
2. If we introduce `Sync Job History` RDO, this won't be an issue.

### **Documents tagging**

The problem with tagging is that Sync (not the external app) should be still responsible for tagging and we still have to use information about existing RIP Job History to tag documents to not break the current RIP workflow.

In destination workspace:

Currently, in order to tag documents in destination workspace, we are creating required Object Types (which are `Relativity Source Case` and `Relativity Source Job`) along with their fields if they don't exist. This is implemented in `DestinationWorkspaceObjectTypesCreationExecutor`. For `Relativity Source Job` we only need Job History name and Job History Artifact ID. Right now Sync is querying for Job History name in `JobHistoryNameQuery` and reading Job History Artifact ID from `Sync Configuration`'s parent object which is `Job History`. Parent Object Type for `Sync Configuration` will have to change to some other Object Type, so we can add `Job History Artifact ID` field explicitly in `Sync Configuration`. Same thing we can do for Job History name.

In source workspace:

To tag documents in source workspace, Sync is using pre-existing RIP Object Type - `Destination Workspace` which comes to the source workspace with RIP application. We could adapt this Object Type in Sync along with its fields similarly as we do with `Relativity Source Case` and `Relativity Source Job`, by implementing new `SourceWorkspaceObjectTypesCreationExecutor`. Then we can check if this Object Type exists in Source Workspace - if not, we'll create it and proceed execution of the job.

### **Parent Object Type for `SyncConfiguration`**

Currently, `Sync Configuration`'s Parent Object Type is RIP `Job History`. This must be obviously changed to some other Object Type. This type should be probably `Workspace`.

## Option 2

Another option is to pass all required RDO GUIDs (including their fields) to Sync job:

- `Sync Configuration`
- `Destination Workspace`
- `Job History`
- `Job History Error`

This can be a dictionary where the key is RDO/field name and value is a GUID. Dictionary can be injected as a parameter to `SyncJob`. For example:

```cs
public SyncJob(INode<SyncExecutionContext> pipeline, ISyncExecutionContextFactory executionContextFactory, SyncJobParameters syncJobParameters, IProgress<SyncJobState> syncProgress, 
Dictionary<string, Guid> rdoGuids, ISyncLog logger)
{
    ...
}
```

Or to avoid adding another parameter, we could add this dictionary as a property in `SyncJobParameters` (which is already registered in container so it can be easily resolved in configuration classes etc.).

Additionally, Sync should be able to create internal RDOs (that is `Sync Progress` and `Sync Batch`) on its own when they don't exist in workspace. This can be implemented as new step, for example `SourceWorkspaceObjectTypesCreationExecutor`.

## Consequences

There are couple of options to discuss and choose the best solution.
