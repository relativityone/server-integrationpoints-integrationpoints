# SYNC - IAPI 2.0 Integration Design

## Status

Proposed

## Context

One of our goals planned for this year is to integrate with new IAPI 2.0. Right now SYNC is using old implementation which is working in Sync Agent context. Current IAPI implementation has limitations and is hard to maintain. In last year IAPI Team started to re-design IAPI and implemented IAPI 2.0, which should be more reliable, fast and in overall better product. SYNC which heavy relys on IAPI wants to benefit from it as soon as possible. This document is the result of spike analysis how integration should be approach and which pit falls we should be aware of.

## IAPI Key Decisions

IAPI 2.0 - <https://git.kcura.com/projects/DTX/repos/relativity-import/browse>

IAPI 2.0 SDK - <https://git.kcura.com/projects/DTX/repos/relativity-import-sdk/browse>

### Key differences IAPI 2.0 vs IAPI 1.0

- IAPI 2.0 is RAP
- IAPI 2.0 Import has it's own Agent and manage jobs on it's own
- Import requires LoadFile creation instead of streaming
- IAPI 2.0 doesn't transfer files anymore (only linking is possible)
- Status is tracked by Kepler calls (polling) or using Service Bus subscribe
- IAPI 2.0 doesn't send ItemLevelErrors events anymore (we read them in pack)
- IAPI 2.0 handles Pasue, Cancel, Drain-Stop on it's own

Note: Streaming in IAPI 2.0 is on the roadmap so in the feature we are going to align into it for performance improvements.

## SYNC Re-Architecture Decision

Present Relativity.Sync architecture process whole sync in batches (25k each) one after another in `*SynchronizationExecutor`:

- Create ImportAPI Job
- Import Data in Destination Workspace using streamed Source Workspace data
- Tag Documents in Source Workspace and in Destination WOrkspace
- Create Item Level Errors if any
- Aggregate results

SYNC key features:

- Documents Synchronization
- Images Synchronization
- RDO Synchronization

### SYNC Changes

#### Key Decisions

- We configure single Import Job
- Sync Batches will be read using DataReader and write to LoadFile
- Every Sync Batch will be separate DataSource for Import Job
- Status will be updated using Service Bus Subscription

#### Deployments with backward compatibility

We are going to hide IAPI 2.0 flow behind a toggle - `Relativity.Sync.Toggles.EnableIAPIv2FlowToggle`

Every flow has it's own pipeline which is decoupled from others. Thanks to that we can make decision about IAPI 2.0 use on the pipeline level which enables us to integration incrementally. We are going to start from _Documents Synchronization_ flow and our goal is to **go to production as soon as possible**.

IAPI 2.0 flow would be separate pipeline - [SYNC Pipelines](https://git.kcura.com/projects/DTX/repos/relativitysync/browse/Source/Relativity.Sync/Pipelines). Thanks to that we would have possibility to narrow IAPI 2.0 usage in initial phase to basic scenarios e.g. Documens push without Files, Extracted Text, Single/Multi Object Fields. Once it will be battle tested we start to cover next flows.

#### Documents Synchronization Pipeline

```cs
public void BuildFlow(IFlowBuilder<SyncExecutionContext> flowBuilder)
{
    flowBuilder.AddRoot<SyncRootNode>()
        .AddChild<PreValidationNode>()
        .AddChild<DestinationWorkspaceObjectTypesCreationNode>()
        .AddChild<PermissionsCheckNode>()
        .AddChild<ValidationNode>()
        .AddChild<DataSourceSnapshotNode>()
        .AddChild<SyncMultiNode>()
        .ForLastChild()
        .AddChild<DocumentJobStartMetricsNode>()
        .AddChild<DestinationWorkspaceTagsCreationNode>()
        .AddChild<SourceWorkspaceTagsCreationNode>()
        .AddChild<DataDestinationInitializationNode>()
        .ForParent()
        .AddChild<DestinationWorkspaceSavedSearchCreationNode>()
        .AddChild<ConfigureDocumentSynchronizationNode>()
        .AddChild<SnapshotPartitionNode>()
        .AddChild<BatchDataSourcePreparationNode>()
        .AddChild<IAPIv2_DocumentSynchronizationNode>()
        .AddChild<DataDestinationFinalizationNode>();
}
```

SYNC Architecture is extensible so we are not planning to change old logic, but only add new "bricks" to the pipeline

Additional Nodes:

- `ConfigureDocumentSynchronizationNode`
- `BatchDataSourcePreparationNode`
- `IAPIv2_DocumentSynchronizationNode`

#### **ConfigureDocumentSynchronizationNode**

This node will be responsible for IAPI Job Configuration (in IAPI 2.0 there is single job which is fed with multiple DataSources)

- Import Job creation
- Import Job Configuration will be prepared based on Sync Configuration
- Import Job Preparation Error Handling

_Note: This flow won't be Drain-Stopped_

```cs
internal class ConfigureDocumentSynchronizationExecutor : IExecutor<IConfigureDocumentSynchronizationConfiguration>
{
    ...

    public async Task<ExecutionResult> ExecuteAsync(IConfigureDocumentSynchronizationConfiguration configuration, CompositeCancellationToken token)
    {
        ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create(); // Settings configured based on SyncConfiguration

        using (IImportJobController importJob = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
        using (IDocumentConfigurationController documentConfiguration = await _serviceFactory.CreateProxyAsync<IDocumentConfigurationController>().ConfigureAwait(false))
        {
            Response response = await importJob.CreateAsync(
                    workspaceID: configuration.DestinationWorkspaceArtifactId,
                    importJobID: configuration.ExportRunId,
                    applicationName: _parameters.SyncApplicationName,
                    correlationID: _parameters.WorkflowId)
                .ConfigureAwait(false);

            response = await documentConfiguration.CreateAsync(
                configuration.DestinationWorkspaceArtifactId,
                configuration.ExportRunId,
                importSettings).ConfigureAwait(false);

            await importJob.BeginAsync(configuration.DestinationWorkspaceArtifactId, Guid.Parse(_parameters.WorkflowId)).ConfigureAwait(false);
        }

        return ExecutionResult.Success();
    }
}
```

We plan to use ExportRunId (ExportAPI guid identifier) as `ImportJobID`. When the job will be configured we call `importJob.BeginAsync` which start the job and waiting for Data Sources.

#### **BatchDataSourcePreparationNode**

This Node will be called after `SnapshotPartitionNode` so we expect that all batches will be created and persisted in _SyncBatch RDO_. In this node we plan to iterate through all batches and write Source Workspace Data using `ISourceWorkspaceDataReader` to files.

Files will be stored in Workspace fileshare location in newly created folder "Sync" e.g. _\\\\emttest\\DefaultFileRepository\\EDDS<Workspace_ID>\\Sync_. Every Batch should have it's own folder structure, because in the feature Extracted Text files will be also stored there.

```tree
+-- Workspace FileShare
│   Sync
+-- <ExportRunId_Guid>
|   |
│   +-- <Batch_Guid>
|       |   <Batch_Guid>.dat
|       |   ...
|   |
|   +-- ...
```

In current _SyncBatch_ object structure we only have ArtifactID so it will be extended, we also introduce another state in `BatchStatus` enum - `Generated` which will inform that the batch was written to LoadFile

_Optional: Right now we don't know if it will be applicable, but we can consider to introducing another field with written LoadFile Path if we decide it's necessary._

```cs
internal class BatchDataSourcePreparationExecutor : IExecutor<IBatchDataSourcePreparationConfiguration>
{
    ...

    public async Task<ExecutionResult> ExecuteAsync(IBatchDataSourcePreparationConfiguration configuration, CompositeCancellationToken token)
    {
        List<int> batchesIds = (await _batchRepository
            .GetAllBatchesIdsToExecuteAsync(
                configuration.SourceWorkspaceArtifactId,
                configuration.SyncConfigurationArtifactId,
                configuration.ExportRunId)
            .ConfigureAwait(false))
            .ToList();

        string syncPath = // retrieve Sync folder path or with export already??

        using (IImportSourceController importSource = await _serviceFactory.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
        using (IImportJobController importJob = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
        {
            foreach (var batchId in batchesIds)
            {
                IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId).ConfigureAwait(false);

                string syncBatchPath = Path.Combine(syncPath, $"{batch.BatchGuid}.dat");

                using (var reader = _dataReaderFactory.CreateNativeSourceWorkspaceDataReader(batch, token.AnyReasonCancellationToken))
                using (StreamWriter writer = new StreamWriter(syncBatchPath))
                {
                    while (reader.Read())
                    {
                        // write lines
                    }
                }

                DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
                    .ForLoadFile(syncBatchPath)
                    .WithDefaultDelimiters()
                    .WithoutFirstLineContainingHeaders()
                    .WithEndOfLineForWindows()
                    .WithStartFromBeginning()
                    .WithDefaultEncoding()
                    .WithDefaultCultureInfo();

                await importSource.AddSourceAsync(
                        configuration.DestinationWorkspaceArtifactId,
                        configuration.ExportRunId,
                        batchGuid,
                        dataSourceSettings)
                    .ConfigureAwait(false);

                // Change Batch Status to Generated
            }

            var response = await importJob.EndAsync(configuration.DestinationWorkspaceArtifactId, configuration.ExportRunId).ConfigureAwait(false);
        }

        return ExecutionResult.Success();
    }
}
```

In this node we plan to write SyncBatches data into LoadFiles and add them to Import Job as Data Source. In first phase we are going to generate LoadFiles one after another (to avoid deadlocks on reading from Source Workspace), but in the future we plan to paralelize this process.

Drain-Stop will be supported in this flow - When drain-stop occurs we don't want to process further batches and on resume we are going to process only those which don't have `Generated` status. We need to make decision how to handle Drain-Stop for batch processed in progress - Extend `IsCancelled` which means that CancellationToken was requested (Drain-Stop token). Thanks to that we could monitor if Batch LoadFile should be added to Import Job and if Reader would be close but marked `IsCancelled` we could update Batch starting index to resume from this record when the job would be resumed.

Once Import Job will be fed with all Batch Data Source we call `importJob.EndAsync` which close the job configuration.

Execution constrain should check if all batches assigned to Sync Job are in `Generated` status and if so we should skip this node

IMPORTANT: LoadFiles generation needs to be as close to "Default" IAPI seetings (EndOfLines, Delimiters etc.) and SHOULD NOT have columns header - Mapping will be configured in Import Job, what we need are only data. It simplifies LoadFile generation.

Note: Statistics should be calculated based on LoadFile size and will be only for _Total Metadata Bytes Size_ - There is no possibility to monitor _Transferred Metadata Bytes Size_

Note: We should handle Item Level Errors when `SyncItemLevelErrorException` will be thrown

#### **DocumentSynchronizationMonitorExecutor**

This node will be executed when the Import Job will be in progress, so the responsibility of this node is to monitor the status:

- Check if Import Job was finished
- Update Sync Progress
- Handle Item Level Errors
- Tag Documents

1. Import Job Status

    Import Job End Status will be checked using polling method in while loop:

    ```cs
    ValueResponse<ImportDetails> result = null;
    do
    {
        ...

        await Task.Delay(TimeSpan.FromSeconds(10));

        result = await importJob.GetDetailsAsync(
                configuration.DestinationWorkspaceArtifactId,
                Guid.Parse(_parameters.WorkflowId))
            .ConfigureAwait(false);

        ...
    }
    while (!result.Value.IsFinished);
    ```

2. Update Sync Progress

    Sync Progress will be updated using Relativity Service Bus. Below example show polling-base implementation:

    ```cs
    ValueResponse<ImportDetails> result = null;
    do
    {
        ...
        var progress = (await importJob.GetProgressAsync(
                configuration.DestinationWorkspaceArtifactId,
                Guid.Parse(_parameters.WorkflowId))
            .ConfigureAwait(false))
            .Value;

        await jobProgress.UpdateJobProgressAsync(progress.ImportedRecords, progress.ErroredRecords).ConfigureAwait(false);
        ...
    }
    while (!result.Value.IsFinished);
    ```

    Progress should take under consideration Item Level Errors which were thrown during DataSource reading

3. Handle Item Level Errors

    Item Level Errors will be handled only if DataSorce processing will be finished. It has drawback that the customer will see the Item with Errors on the UI, but detailed error messages will be added with delay, but we accept this change. We made this decision because it's easier to handle all Item Level Errors at once instead of monitor the state and process them in parts.

    Errors will be retrieved using `IImportSourceController.GetErrorsAsync` in batches and handled by `IJobHistoryErrorRepository.MassCreateAsync` in batches (e.g. by 5000).

4. Documents Tagging

    This feature is most problematic to achieve using new IAPI 2.0 implementation, because we don't have information about records which were processed. Missing this feature disable also Saved Search creation for pushed documents in Destination Workspace.

    It can be achieved, but the process would be highly inefficient, because it would require another Export API snapshot with Documents in SavedSearch and then we would need to iterate over the documents and filter out errored documents. It's worth to consider.

Full PoC Draft Implementation:

```cs
internal class DocumentSynchronizationMonitorExecutor : IExecutor<IDocumentSynchronizationMonitorConfiguration>
{
    ...

    public async Task<ExecutionResult> ExecuteAsync(IDocumentSynchronizationMonitorConfiguration configuration, CompositeCancellationToken token)
    {
        using (IImportJobController importJob = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
        using (IImportSourceController importSource = await _serviceFactory.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
        {
            DataSources dataSources = (await importJob
                    .GetSourcesAsync(configuration.DestinationWorkspaceArtifactId, Guid.Parse(_parameters.WorkflowId))
                .ConfigureAwait(false)).Value;


            IJobProgressUpdater jobProgress = _jobProgressFactory.CreateJobProgressUpdater();

            ValueResponse<ImportDetails> result = null;
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(10));

                result = await importJob.GetDetailsAsync(
                        configuration.DestinationWorkspaceArtifactId,
                        Guid.Parse(_parameters.WorkflowId))
                    .ConfigureAwait(false);

                var progress = (await importJob.GetProgressAsync(
                        configuration.DestinationWorkspaceArtifactId,
                        Guid.Parse(_parameters.WorkflowId))
                    .ConfigureAwait(false))
                    .Value;

                await jobProgress.UpdateJobProgressAsync(progress.ImportedRecords, progress.ErroredRecords).ConfigureAwait(false);

                List<Guid> processedSourceIds = new List<Guid>();
                foreach (var sourceId in dataSources.Sources)
                {
                    var dataSource = (await importSource.GetDetailsAsync(
                            configuration.DestinationWorkspaceArtifactId,
                            Guid.Parse(_parameters.WorkflowId),
                            sourceId)
                        .ConfigureAwait(false))
                        .Value;

                    if (dataSource.State == DataSourceState.CompletedWithErrors)
                    {
                        // Read/Add Item level errors
                        // Tagging
                    }
                    else if (dataSource.State == DataSourceState.Completed)
                    {
                        // Tagging
                    }
                }
            }
            while (!result.Value.IsFinished);

            return ExecutionResult.Success();
        }
    }
}
```

IMPORTANT: We should keep SyncBatch RDO consistent with Import Job status

We should re-think if Drain-Stop is applicable in this node. In theory it can be used in Tagging and Item Level Error Handling, but maybe it's not worth and those operation are not long enough to not met 5 minutes shutdown requirements.

Note: We need to disable _Natives File Location Validation_ using `AdvancedNativeSettings.ValidateFileLocation`

#### **Cleanup**

We should cleanup created loadfiles in when batches are removed and job was finished.

## Timeline

1. We are going to implement basic scenario for documents push without _Long Text_ fields and without Native files

2. Implement Retry Errors feature

3. Extend Sync job with Long Text Fields (perhaps saved to files and with links in *.dat LoadFile)

4. ADF/ADLS re-architecture to fully cover Sync Documents Flow (with CopyFiles Mode)

5. Integrate with IAPI 2.0 for Images Flow

6. Integrate with IAPI 2.0 for Non-Documents Flow

_Note: After Documents flow will be fully integrated we are going in meantime to increase performance because there is a lot of low-hanging fruits

## Issues

1. ADF/ADLS flow would need to be re-architectured to meet new requirements - there is no more streaming so the paths need to be stored directly in LoadFile and should follow Drain-Stop requirements

2. Tagging require some additional effort in comparison to IAPI 1.0 - it was for free there (we lose Create Saved Search feature)

3. We can't monitor transferred metadata size because we don't have access to information about transferred documents

4. There is high possibility that it won't be possible to transfer only succesffully processed items - but if we decide to do so it won't be possible to paralelize it in the future

5. Since IAPI 2.0 is hosted in Agent our System Tests are not viabale anymore in Sync - previously we were able to run Sync Tests against Local Machine

## Benefits

1. There is option for huge optimalization with _Sync Monitor Agent_ instead of maintaining Agent per Sync Job.

2. Data Source preparing (Sync Batches to LaodFile) maybe be paralelized to increase performance

3. IAPI is finally Pausable and the Drain-Stop handling is moved out of Sync

4. We expect IAPI 2.0 to be more stable and more resilient.

5. No more IAPI Package updates (only SDK left)

6. Communication is Kepler based which simplify the configuration

7. Retrying only part of the job is open now - we can track which batches were processed successfully and remove them from Retry even if the job finished with "Job Failed Error"

8. Feature with Pausing Sync job is now possible

## Documentation

- PoC was built based on `Sample01_ImportNativeFiles.cs` test case located in [Relativity.Import.SDK](https://git.kcura.com/projects/DTX/repos/relativity-import-sdk/browse)

- [PoC Branc](https://git.kcura.com/projects/DTX/repos/relativitysync/browse?at=refs%2Fheads%2FREL-699595-IAPI-v2-integration-poc-DO-NOT-DELETE)

- Service Bus Configuration:
  - IAPI Events - [import-asyncapi.yaml](https://git.kcura.com/projects/DTX/repos/relativity-import/browse/import-asyncapi.yaml)
    - Use Modelina to generate models from *.yaml file [AsyncAPI-Modelina](https://www.asyncapi.com/tools/modelina)
  - Eventing Documentation Guid - [Eventing & Messaging - Event Onboarding Guide](https://einstein.kcura.com/x/auQZF)
  - Web Import/Export Example - [ImportJobStateChanged](https://git.kcura.com/projects/DTX/repos/rie-webapp/browse/Source/RIE.WebApp.Core.Models/Import/ImportJobStateChanged.cs#4,9)
