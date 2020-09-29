# Near-Zero Downtime Architectural Requirements - SYNC Drain Stop

## Status

Proposed

## Context

Relativity ADS applications must accept a set of architectural requirements and caveats in order to perform a Near-Zero Downtime deployment in Relativity environment. One of such requirements is that Agents needs to be able to go down within 5 minutes and then resume work.

### Details

Drain-stop requires shutting down the old process, which will force the termination of outstanding TPL Tasks and background threads.  Don't perform long-running work on a background thread or a TPL task that can't survive the graceful shutdown.

Due to the nature of Agents, a job might take a very long time to complete. Such long-running jobs should be split into discrete units of work so the agent can be restarted within the requirement window.

## Decision

Following drain-stop requirements we are forced to review all of our pipelines steps to ensure we can stop and resume job gracefully.

Analysis would be done based on Native Sync Flow. Sync Job consists of following steps:

_Note_: Metrics are covered in another section

```csharp
flowBuilder.AddRoot<SyncRootNode>()
    .AddChild<DestinationWorkspaceObjectTypesCreationNode>()
    .AddChild<PermissionsCheckNode>()
    .AddChild<ValidationNode>()
    .AddChild<DocumentDataSourceSnapshotNode>()
    .AddChild<SyncMultiNode>()
    .ForLastChild()
    .AddChild<JobStartMetricsNode>()
    .AddChild<DestinationWorkspaceTagsCreationNode>()
    .AddChild<SourceWorkspaceTagsCreationNode>()
    .AddChild<DataDestinationInitializationNode>()
    .ForParent()
    .AddChild<DestinationWorkspaceSavedSearchCreationNode>()
    .AddChild<SnapshotPartitionNode>()
    .AddChild<SynchronizationNode>()
    .AddChild<DataDestinationFinalizationNode>();
```

1. Steps which need to be run every time:

    + DestinationWorkspaceObjectTypesCreationNode
    + PermissionsCheckNode
    + ValidationNode

    We should ensure that in case of Run-Pause-Resume job is still valid (fields and sync objects exist) and all permissions are granted.

2. Data Source Snapshot:

    Responsibilities:

    + Create Data Snapshot via OM
    + Calculate data total size (background process)
    + Set total items count for update

    Once we create Data Snapshot and set total items count for update it can be reused after resume the job. The problem is with total data size calculation. Perhaps we would need to abort it due cancellation and calculate one more time after resuming.

    Following current implementation of `DataSourceSnapshotConfigurationConstrains` we don't run calculation again when Data Snapshot has been created:

    ```csharp
    public Task<bool> CanExecuteAsync(IDocumentDataSourceSnapshotConfiguration configuration, CancellationToken token)
    {
        return Task.FromResult(!configuration.IsSnapshotCreated);
    }
    ```

    **[Obsolete]**
    ~~We could consider similar approach to [REL-445672](https://jira.kcura.com/browse/REL-445672). It enables to narrow total data size calculation to one batch. It could be implemented in `RelativityExportBatcher` where we get documentIds for batch. Then in `*JobEndMetricsService` summarize it same as for other counts.~~

    ~~As a main benefit we could remove total data size calculation which right now violates SoC principle.~~

    ~~Drawback of this approach would be possible performance degradation. We could try to run calculation in background process, in result it could prolong batch cleanup process.~~

    ~~_Note_: I don't think that size calculation takes more than transferring documents in batch, but it needs to be check~~

    **[Proposed]**
    Problem with previous approach is that if one of the batches fail and in result we fail the job then Total Data Size calculation would be calculated only for procceeded batches. As temporary solution we should start Total Data Size calculation every time the job is resumed.

    It could impact overall performance because when the job has been paused in 99% we'll do whole data size calculation in this 1% time, which prolong job completion time.

    Another problem is that we should extract Total Data Size calculation from DataSourceSnapshot because it needs to be calculated on every run, but snapshot is created once per job.

3. Workspace Tag Creation:

    Those steps are responsible for tags creation in Source and Destination Workspace. We assume that _JobHistoryId_ and _Integration Point Name_ don't change in between Pause and Resume. Problem which can occur is related to Source/Destination Workspace Name. If it change the tags will be different after job resuming.

    As proposed approach we could validate in `DestinationWorkspaceTagsCreationExecutionConstrains` whetever Tags for particular workspace exist.

4. Snapshot Partition:

    This step won't run if batches are already created.

5. Data Synchronization:

    This is the most crucial part of whole Sync flow. For now IAPI doesn't support job cancellation so we need to do it on our own.

    Synchronization Executor consists following responsibilities:

    + Transfer documents via IAPI
    + Create Item Level Errors for documents in batch
    + Tag documents in Source and Destination Workspace
    + Report progress
    + Update Batch statistics in RDO

    One of the possible approaches is to close `BatchDataReader` and stop sending next rows. When empty row would be send we would process standard Batch finalization (update failed and transferred items count, write item level errors etc.). Batch could be mark with status **Paused**. SynchronizationExecutor would needs to retrieve _New_ and _Paused_ batches. Paused batch would be processed as first starting from index **TotalItemsCount - TransfferedItemsCount**. Rest flow would be the same as previously.

6. Sync Root:

    + Report Job end metrics -> no metrics should be send (or only about job paused status)
    + Send notifications -> Send notification that job has been paused
    + Run automated workflow trigger -> Perhaps shouldn't be run because job didn't finish (customer assume that job needs to have one of 'ending' status)
    + Job cleanup -> Don't cleanup when Paused

## Metrics

Metrics with partial results shouldn't be send. It would require many changes in our dashboards based on multiple metrics related to one job _CorrelationId_.

+ **Relativity.IntegrationPoints.Job.Start.Type** - Should be send once. Perhaps we would need to receive some information from outside that this job is _Resumed_ to don't send it twice.
+ **Relativity.IntegrationPoints.Job.End.Status** - Shouldn't be send when job has been paused
+ **Relativity.Sync.Data.LongTextStream** - This metrics keeps its running result through the job in memory. The results should be stored in persistent storage after drain stop. **[TBC]**

## RIP Requirements

1. Add job status managing in Schedule Queue.

    Right now when starting the job it appears in the queue and it's picked up by the Agent. We shouldn't delete the job from the queue when it has been paused. We could use for that **StopState** flag from _ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D_. It would enable us to ommit schema changes in DB. Agent wouldn't pick up the job if flag would be set. After clicking resume flag would be off and job could be taken in next polling run.

2. We should disable Integration Point edition when job has been Paused

    Resuming job would end up with running new Sync job so parameters needs to stay unchanged. There are two possible ways:

    + Grey out _Edit_ button when job is in _Pause_ state
    + Display message as prerequisite e.g _"You cannot edit this Integration Point while its paused"_ after clicking _Edit_
    + Add validation on 3rd Step when saving the Integration Point

    Pros & Cons

    + Edit button doesn't belongs to us so it could be hard to provide required logic there (maybe some pre-handler would be resolve this)
    + Validtion on 3rd step require to fill whole information by customer and finally at the end show up the message

3. Create _Pause_ button on Integration Point Summary Page

    For now _Run_ is replaced with _Stop Job_ button. We should consider how it should looks like and when it should be put. From technical perspective it should only take off the flag from paused job in the queue. It should be narrowed only to Sync Jobs

4. Integration Point new run when job is paused

    Two possible ways:

    + Automatically cancel paused job
    + Unable to run new job until we don't stop/finish paused job

## Limitations

+ Sync

  + Data Source Snapshot lives 7 days. We should delete/stop the job after this time
  + Sync job needs to be paused within 5 minutes from point when cancellation occurs. Our bottleneck is IAPI. For now we don't know what is maximum duration for transferring 1 doc (ever logged). We would have: `Maximum Cleanup Time = 5 minutes - 1 doc transfer time` (Write count metrics to batch, tag transferred documents).

    _Note_: Worst case is when 24 999 docs would be transfered - Total Size Calculation and Tagging

  + Sync needs to know if the job is paused-resumed (End metrics send)

+ RIP

  + After 7 days Sync Job should be clean up (Configuration, Batches) somehow

## Consequences

Sync architecture fits well for such problem. It looks like that there were some paperchase for such feature. Before we take a decisions we should gather metrics e.g. from T009 how long does every pipeline step take which enable us to know if we are under the time limit.

+ Pros

  + No additional steps/separate pipeline is required
  + No schema changes in EDDS
  + Only additional RDO would be required: Batch::TotalSizeItemsCount

+ Cons

  + RIP UI changes
  + Validation to prevent Integration Point editing
  + Possible Performance Degration in _SynchronizationNode_
