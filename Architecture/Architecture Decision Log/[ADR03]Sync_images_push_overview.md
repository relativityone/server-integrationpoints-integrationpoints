# Push Images in Sync - Architecture Overview

## Status

Proposed

## Context

### Requirements

+ Decouple images push from RIP → Sync (1 to 1)
+ Pushing covers only Saved Search → Folder

### UI Overview

+ Copy Images: Yes/No
+ Images Precedence:
  + Original Images
  + Produced Images:
    + Production Precedence: List of productions
    + Include Original Images If Not Produced: Yes/No

### How it works

If user wants to push images between workspaces there is two main flows:

+ Push Original Images
+ Push Images from Productions
  + Include Original Images

+ Copy Files To Repository
  + Yes - Images are copied between workspaces
  + No - Images are pushed as links

The case which requires attention is Push Images from Productions. User chooses Productions where images should comes from and in result the pushed image is based on selected productions order. The first one where image exists is choosen. e.g:

Images to push: _img1, img2, img3_

Prod1: _img1_

Prod2: _img1, img3_

Destination Workspace:

+ img1 (Prod1)
+ img2 (If Include Original Images has been selected)
+ img3 (Prod2)

If image doesn't exist - document won't be pushed, same situation is when Produced is choosen and Include Original Images is not set

__Note:__ When images are pushed only _Control Number_ is in Fields Map

## Decision

### Sync Architecture

#### Sync Configuration

Sync configuration needs to be extended by couple of additional fields which enable to configure image Sync job.

Configuration Fields:

+ ImageImport - determine if job is Image Sync
+ ImagePrecedence - List of productions, where images come from if applicable
+ IncludeOriginalImages - determine if push original image if it doesn't exist in selected productions
+ ImageFileCopyMode - determine if copy physical file or link only

__Note__: These properties can be taken directly from ImportSettings

#### **ImageDataReader**

Columns:

+ ControlNumber
+ ImageFileName
+ ImageFileLocation

Right now retrievieng Image path for document is done by ISearchManager:

+ For Original Images - _ISearchManager.RetrieveImagesForDocuments_
+ For Productions - _ISearchManager.RetrieveImagesForProductionDocuments_

_ADR Reference_: [ADR04]Sync_images_push_redundant_reader_fields

#### **Handling multiple images per document**

Single document can contain multiple images, and current `BatchDataReader` implementation does not support that, because `GetBatchEnumerable` returns one row per document, and we need to return one row for each image. That's why we created specific implementation of `ImageBatchDataReader`, which counts number of images in document and then creates data row for each image file.

#### **Pipeline**

We should create two new pipelines _SyncImagesRunPipeline_ and _SyncImagesRetryPipeline_. The one step which is different is _SynchronizationNode_ which should be replaced with _ImagesSynchronizationNode_. There is couple of differences which should be covered in some other steps like ValidationNode and DataSourceSnapshotNode

```csharp
internal sealed class SyncImagesRunPipeline : ISyncPipeline
{
    public void BuildFlow(IFlowBuilder<SyncExecutionContext> flowBuilder)
    {
        flowBuilder.AddRoot<SyncRootNode>()
            .AddChild<DestinationWorkspaceObjectTypesCreationNode>()
            .AddChild<PermissionsCheckNode>()
            .AddChild<ValidationNode>()
            .AddChild<DataSourceSnapshotNode>()
            .AddChild<SyncMultiNode>()
            .ForLastChild()
            .AddChild<JobStartMetricsNode>()
            .AddChild<DestinationWorkspaceTagsCreationNode>()
            .AddChild<SourceWorkspaceTagsCreationNode>()
            .AddChild<DataDestinationInitializationNode>()
            .ForParent()
            .AddChild<DestinationWorkspaceSavedSearchCreationNode>()
            .AddChild<SnapshotPartitionNode>()
            .AddChild<ImagesSynchronizationNode>()
            .AddChild<DataDestinationFinalizationNode>();
    }
}
```

__ValidationNode__:

+ Validation Step needs to be added to determine if Copy Images has been selected, fields mapping should contain only Control Number

_ADR Reference_: [ADR07]Sync_images_push_fields_map_validation

__DataSourceSnapshotNode__:

DataSourceSnapshotExecutor shouldn't change from functional point of view. Only thing we should take care of is image size calculation. Current implementation of _DataSourceSnapshotExecutor_:

```csharp
...
ExportInitializationResults results;
try
{
    using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
    {
        results = await objectManager.InitializeExportAsync(configuration.SourceWorkspaceArtifactId, queryRequest, 1).ConfigureAwait(false);
        _logger.LogInformation("Retrieved {documentsCount} documents from saved search.", results.RecordCount);

        Task<long> calculateNativesTotalSizeTask = Task.Run(() => _nativeFileRepository.CalculateNativesTotalSizeAsync(configuration.SourceWorkspaceArtifactId, queryRequest), token);
        _jobStatisticsContainer.NativesBytesRequested = calculateNativesTotalSizeTask;
    }
}
...
```

Probably we should add another property to IJobsStatisticsContainer - ImagesBytesRequested. I would think about moving this code outside of DataSourceSnapshotExecutor and calculate it in Node responsible for gathering metrics for job. In otherwise if we won't decide to move it out we should calculate it in another try-catch statement (or catch exception in INativeFileRepository) to avoid situation that Natives/Images fail our job if some error during calculation occurs.

_ADR Reference_: [ADR06]Sync_images_push_image_size_calculation

As for export itself, it needs to include identifier field to enable tagging in destination workspace and provide correct names for pushed docuemnts.

#### **ImagesSynchronizationNode**

This step needs to be replaced completely and it'll be most complicated task. We need to replace following line of code in method ExecuteSynchronizationAsync and provide proper ISynchronizationConfiguration

IAPI Import Job creation requires:

+ Specific job type creation (NewNativeDocumentImportJob, NewImageImportJob)
+ Job Configuration
+ DataReader

```csharp
...
using (IImportJob importJob = await _importJobFactory.CreateImportJobAsync(configuration, batch, token).ConfigureAwait(false))
...
```

On first look we should provide new method to IImportJobFactory → CreateImageImportJob, and probably rename old one to CreateNativeImportJob. There we should create completely new DataReader (within same functionality as BatchDataReader for Natives, but his own Item Level Error handling and Row Builder)

_ADR Reference_: [ADR05]Sync_images_push_ImagesSynchronizationNode_design

#### **Metrics**

Adding Images push requires metrics rethink:

+ Which metrics should be gathered
+ Right now job metrics are tightly coupled to Natives. We need to think how to intersperse images metrics there

_ADR Reference_: [ADR08]Sync_images_push_metrics_design

## Consequences

Decoupling Images push to Sync enables us to move next part of syncing documents to new better code base and get better monitoring, logging and performance.

Sync architecture after retry implementation (_[ADR01]Creating_alternative_versions_of_Sync_pipeline_) is more flexible for changes.

Moving code to new Sync also enable us to validate execution paths and ommit them if they are not needed anymore

## Links

+ Einstein Architecture Overview: <https://einstein.kcura.com/x/3uobD>
