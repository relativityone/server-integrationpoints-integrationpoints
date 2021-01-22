# RIP drain stop for Other Import Providers

## Status

Proposed

## Context

Due to new requirement for **Near Zero Downtime** we need to be able to pause the RIP job in under 5 minutes, upgrade RIP and resume the job.

Overall Drain-Stop approach in RIP was covered very well in all other Drain-Stop ADRs. In this one i focus on Other Provider specific flow.

Import Job consists of two "steps":

- Sync Manager (It was covered in _ADR05_)
- Sync Worker

## Sync Manager

Beside the changes covered in previous ADRs we should think about Drain-Stopping `SyncManager` itself. What if Pause would be triggered during execution. We should think about another status to prevent any other Agent to pickup child job created by SyncManager. We could first create the child jobs with status e.g. `NotReady`, and when all of them will be created only then take off the flag and enable them for picking up.

If Pause would be triggered we could remove all created child jobs and when resuming run `SyncManager` once again. This degradade the performance, but make us safe with job consistency and potential data loss.

## Sync Worker

`SyncWorker` manages the import itself. It deserializes `JobDetails` column, from which it extracts a list of IDs to import in given batch.

Sample `JobDetails`:

```{json}
{
  "BatchInstance":"24975ce6-e89d-4b06-87aa-4ba1b88323ab",
  "BatchParameters":
  [
    "Text_1",
    "Text_2"
  ]
}
```

By the IDs all other values are read by Provider in `SyncWorker.ExecuteImport`:

```{csharp}
...
using (IDataReader sourceDataReader = sourceProvider.GetData(sourceFields, entryIDs, configuration))
{
  SetupSubscriptions(dataSynchronizer, job);
  IEnumerable<IDictionary<FieldEntry, object>> sourceData = GetSourceData(sourceFields, sourceDataReader);
  JobStopManager?.ThrowIfStopRequested();
  dataSynchronizer.SyncData(sourceData, fieldMaps, destinationConfiguration);
}
...
```

Going through the code we come to `ImportService.PushBatchIfFull`, where following line is executed:

```{csharp}
IDataReader sourceData = _batchManager.GetBatchData();
```

This `IDataReader` is created based on data retrieved by the Provider, which is passed to IAPI Import Job. DataReader is created based on DataTable. We could introduce wrapper for this Reader:

```{csharp}
public class PausableDataReader : IDataReader
{
  ...
}
```

It would enable us to implement our own logic for stop sending next rows if Paused has been triggered.

We could collect within this reader processed IDs. Then we could make this IDs accessible from `RDOSynchronizer` where we could also pass `Job` as field. Then we could modify `JobDetails` by removing processed IDs.

After that in ScheduleQueue instead of removing job from the queue, we would update `JobDetailes`.

When resuming the job no changes would be required, because the Batch Job would already have records which should be processed.

## Consequences

Pros:

- This is transparent for all Other Providers, no changes in contracts required
- At first glance this approach could be applicable for all Import flows, because the code which require modifications is comon for all of them.
  - Import LoadFile
  - FTP
  - Other Providers

I don't see any deltas for this approach. Perhaps we don't even send any of the records twice.