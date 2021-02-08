# RIP drain stop - LDAP provider

## Status

Proposed

## Context

Due to new requirement for Near Zero Downtime we need to be able to pause the RIP job in under 5 minutes, upgrade application and resume the job.

Drain stopping LDAP provider is almost identical to drain stopping other providers, which is covered in separate ADR (Other Providers). It basically requires modifying `JobDetails` field by removing items that were already transferred. In case of LDAP provider, single entity is identified by entity full name (e.g. `Andrew Zipper`). However, there is one problematic part in LDAP job, described below.

## Link Managers Job

Currently at the end of the LDAP job, `RdoEntitySynchronizer` checks if there is a need to link entities to managers and adds another RIP job to the queue. Job parameters for this new job contains dictionary - Entity to Manager map. The logic responsible for running this job is implemented in `SyncEntityManagerWorker`. Implementation is shady and is quite complicated. At first glance it looks like there is some kind of batching:

```cs
//check if all tasks are done for this batch yet
bool isPrimaryBatchWorkComplete = _managerQueueService.AreAllTasksOfTheBatchDone(job, taskTypeExceptions: new[] { TaskType.SyncEntityManagerWorker.ToString() });
if (!isPrimaryBatchWorkComplete)
{
   new TaskJobSubmitter(JobManager, job, TaskType.SyncEntityManagerWorker, BatchInstance).SubmitJob(jobParameters);
   return;
}
```

However when we go further, we can see that it deserializes job details and always creates new temp table (using `ManagerQueueService.GetEntityManagerLinksToProcess` method) with whole entity to manager mapping. Old temp tables (older than 72 hours) are cleaned-up before new table is created (`CreateEntityManagerResourceTable.Execute` method).
Then it reads mapping from that table, updates `[LockedByJobID]` and processes all entities and their managers at once, without any batching. ImportAPI is reconfigured to overlay and set Entity/Manager links. Data is loaded using `IDataSynchronizer`.

## Solution

It looks like we can change that implementation to do the following:

- when `CancellationToken` is raised, stop processing more entities (by returning `false` from `IDataReader.Read`)
- gather Entities IDs which have been already transferred (this might be the hardest part) from `IDataSynchronizer`
- delete transferred records from temp table and set `[LockedByJobID]` to -1 for all the rest of them
- delete transferred records from `JobDetails` (this should work out of the box when we implement drain stop for "Other Providers")

When job is resumed, it will process only entities which were not deleted from `JobDetails` and temp table.

## Consequences

Main advantage of proposed approach is that it shouldn't require major architectural changes. However, it might be tricky to implement as the code is old and poorly tested. Also testing of LDAP job with Managers entities might not be so simple, because it will require to setup environment with proper test data, accessible from Test VM.
