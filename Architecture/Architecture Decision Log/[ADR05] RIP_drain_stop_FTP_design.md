# RIP drain stop for FTP provider (and other import providers)

## Status

Proposed

## Context

Due to new requirement for **Near Zero Downtime** we need to be able to pause the RIP job in under 5 minutes, upgrade RIP and resume the job.

Current architecture uses two different flows to achieve the end result which documents are imported.

First, we create a **parent job**. This job task is taken over by `SyncManager` which creates an instance of the import provider.

```cs
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

**SyncManager** calls `GetBatchableIds` and iterates over the result with a `foreach` loop:

```cs
// BatchManagerBase<T>
public virtual long BatchTask(Job job, IEnumerable<T> batchIDs)
{
    long count = 0;
    var list = new List<T>();
    foreach (var id in batchIDs)
    {
        //TODO: later we will need to generate error entry for every item we bypass
        if ((id != null) && id is string && (id.ToString() != string.Empty))
        {
            list.Add(id);
            count += 1;
            if (list.Count == BatchSize)
            {
                CreateBatchJob(job, list);
                list = new List<T>();
            }
        }
        else
        {
            LogMissingIdError(count);
        }
    }
    if (list.Any())
    {
        CreateBatchJob(job, list);
    }
    return count;
}
```

As shown, it creates a new child job for each list of size `BatchSize` (1000 items). The list of IDs is serialized in the job parameters, meaning, it is stored in `AgentScheduleQueue` table.

After child jobs are created, `SyncManager` role and job ends. Child jobs have the parent job Id and it is parent job that is reported in the UI.

## SyncWorker

`SyncWorker` manages the import itself. It deserializes `JobDetails` column, from which it extracts a list of Ids to import in given batch.

It then passes that list into `IDataSourceProvider.GetData`. Each batch gets new instance of the provider, so there is no way to do any caching between batches. All batches report to the same JobHistory.

## Known pitfalls

RIP providers in general (besides Sync) don't handle stopping well. Fixing that should be a priority.

One of the best ways to go about it would be wrapping reader returned by `IDataSourceProvider.GetData` with a reader that can be cancelled/stopped and after that its `Read` method should return `false` - this will cause IAPI to stop processing and finalize the job, including reporting the number of successfully sent items.

## Decision

Since there is no one single perfect solution, below are 2 proposed solutions with shortest lists of cons. They are vastly different, each brings its own list of tradeoffs.

Both solutions should scale to work with other providers out of the box.

Both solutions also allow the job to paused virtually indefinitely.

# Tagging sent documents

This approach relies on the same mechanism that is used in Sync workflow - each pushed document is tagged with JobHistory object that indicates current RIP job.

Tagging should be done in the `OnRaiseJobPostExecute` method, always. For this to work, we would need to implement tracing of imported documents.

Tagging command size should not be an issue, since the batch size for custom providers is 1000, and in Sync we tag 10 000 at once.

## Pausing the job

Pausing would be the same as stopping: all child jobs get notified and stop processing, then removed from the `AgentScheduleQueue`.

Only difference would be marking JobHistory as `Paused` in some way to enable UI to display `Resume` button.

## Resuming the job

On resume, the previous JobId would be passed as argument. Here we have two options:

- reuse previous JobHistory
- create new one and put the paused job id as ParentJob

`SyncManager` would filter out Ids that were imported in the paused job in `BatchTask` method. This would be based on `ObjectManager` query to find all tagged documents.

After that, the remaining ids would be batched as usual.

## Consequences

Pros:

- very easy concept to reason about
- opens a path for retrying of custom providers

Cons:

- lowers performance - tagging takes time
  - the impact would not be as big, since the batches are run in parallel by multiple agents
- tagging can fail

# Editing JobDetails

This approach relies on adding new states to RIP job StopState: Pausing and Paused. Agent code would have been arranged to accommodate that and ignore paused jobs.

## Pausing the job

Each child job should get paused.

`SyncManager` should perform these steps:

- set job `StopState` to `Pausing`
- stop the import in a graceful fashion
- edit `JobDetails` for given batch by deserializing the ID list and removing first N elements, where N is number of transferred elements.
  - to consider: removing items based on Ids, not based on the order of the list. Each batch imports only 1000 elements, so storing the ids of processed elements would not be very heavy (it would be required for the tagging approach to work)
- set job `StopState` to `Paused`

After the batch job is paused, agent should release the job - set `[LockedByAgentID]` to `null`.

## Resuming the job

Parent job Id should be passed as argument the `Resume` method which should change the `StopState` to `None` on all `Paused` jobs that have that Id as `ParentJob` or `RootJob`. 

`SyncManager` does not do anything when resuming - resumed jobs are polled by agents as usual and with modified `JobDetails` continue exactly where they stopped. 

## Agent changes

`getNextJob.sql` would have to be modified to ignore paused jobs:

```sql
-- ..
WHERE
    (q.[LockedByAgentID] IS NULL AND q.[StopState] <> 4) -- ignore paused jobs
    AND q.[AgentTypeID] = @AgentTypeID
    AND q.[NextRunTime] <= GETUTCDATE()
    AND c.ResourceGroupArtifactID IN (@ResourceGroupArtifactIDs)
```

## Consequences

Pros:

- performance - each batch lives as a separate entity and remembers its own progress, no additional work to do
- would play nicely with our SYNC drain stop which would benefit that `Paused` state for jobs
- skips `SyncManager` on resuming

Cons:

- requires modifications of Agent code
- adds quite complex flow
- blocks any other way of doing pause in other flows: import, entities, etc.
