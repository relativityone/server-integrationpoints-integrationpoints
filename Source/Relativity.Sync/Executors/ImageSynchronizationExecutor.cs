using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors
{
	internal class ImageSynchronizationExecutor : SynchronizationExecutorBase<IImageSynchronizationConfiguration>
	{
		public ImageSynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository,
			IJobProgressHandlerFactory jobProgressHandlerFactory, IDocumentTagRepository documentsTagRepository,
			IFieldManager fieldManager, IFieldMappings fieldMappings, IJobStatisticsContainer jobStatisticsContainer,
			IJobCleanupConfiguration jobCleanupConfiguration,
			IAutomatedWorkflowTriggerConfiguration automatedWorkflowTriggerConfiguration,
			Func<IStopwatch> stopwatchFactory, ISyncMetrics syncMetrics, ISyncLog logger,
			IUserContextConfiguration userContextConfiguration)
			: base(importJobFactory, BatchRecordType.Images, batchRepository, jobProgressHandlerFactory, documentsTagRepository, fieldManager,
			fieldMappings, jobStatisticsContainer, jobCleanupConfiguration, automatedWorkflowTriggerConfiguration, stopwatchFactory, syncMetrics, userContextConfiguration, logger)
		{
		}

		protected override Task<IImportJob> CreateImportJobAsync(IImageSynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
		{
			return _importJobFactory.CreateImageImportJobAsync(configuration, batch, token);
		}

		protected override void UpdateImportSettings(IImageSynchronizationConfiguration configuration)
		{
			configuration.IdentityFieldId = GetDestinationIdentityFieldId();

			IList<FieldInfoDto> specialFields = _fieldManager.GetImageSpecialFields().ToList();
			configuration.ImageFilePathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.ImageFileLocation);
			configuration.FileNameColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.ImageFileName);
			configuration.IdentifierColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.ImageIdentifier);
		}

		protected override void ChildReportBatchMetrics(int batchId, BatchProcessResult batchProcessResult, TimeSpan batchTime, TimeSpan importApiTimer)
		{
			_syncMetrics.Send(new ImageBatchEndMetric()
			{
				TotalRecordsRequested = batchProcessResult.TotalRecordsRequested,
				TotalRecordsTransferred = batchProcessResult.TotalRecordsTransferred,
				TotalRecordsFailed = batchProcessResult.TotalRecordsFailed,
				TotalRecordsTagged = batchProcessResult.TotalRecordsTagged,
				BytesNativesTransferred = batchProcessResult.FilesBytesTransferred,
				BytesMetadataTransferred = batchProcessResult.MetadataBytesTransferred,
				BytesTransferred = batchProcessResult.BytesTransferred,
				BatchImportAPITime = importApiTimer.TotalMilliseconds,
				BatchTotalTime = batchTime.TotalMilliseconds,
			});
		}

		protected override Task<TaggingExecutionResult> TagDocumentsAsync(IImportJob importJob, ISynchronizationConfiguration configuration,
			CompositeCancellationToken token)
		{
			Task<TaggingExecutionResult> destinationDocumentsTaggingTask = TagDestinationDocumentsAsync(importJob, configuration, token.StopCancellationToken);
			Task<TaggingExecutionResult> sourceDocumentsTaggingTask = TagSourceDocumentsAsync(importJob, configuration, token.StopCancellationToken);

			TaggingExecutionResult sourceTaggingResult = await sourceDocumentsTaggingTask.ConfigureAwait(false);
			TaggingExecutionResult destinationTaggingResult = await destinationDocumentsTaggingTask.ConfigureAwait(false);

			TaggingExecutionResult taggingExecutionResult = TaggingExecutionResult.Compose(sourceTaggingResult, destinationTaggingResult);

			return taggingExecutionResult;
		}

		private async Task<TaggingExecutionResult> TagDestinationDocumentsAsync(IImportJob importJob, ISynchronizationConfiguration configuration,
			CancellationToken token)
		{
			_logger.LogInformation("Start tagging documents in destination workspace ArtifactID: {workspaceID}", configuration.DestinationWorkspaceArtifactId);
			List<string> pushedDocumentIdentifiers = (await importJob.GetPushedDocumentIdentifiersAsync().ConfigureAwait(false)).ToList();
			_logger.LogInformation("Number of pushed documents to tag: {numberOfDocuments}", pushedDocumentIdentifiers.Count);
			TaggingExecutionResult taggingResult =
				await _documentsTagRepository.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(configuration, pushedDocumentIdentifiers, token).ConfigureAwait(false);

			_logger.LogInformation("Documents tagging in destination workspace ArtifactID: {workspaceID} Result: {result}", configuration.DestinationWorkspaceArtifactId,
				taggingResult.Status);

			return taggingResult;
		}

		private async Task<TaggingExecutionResult> TagSourceDocumentsAsync(IImportJob importJob, ISynchronizationConfiguration configuration,
			CancellationToken token)
		{
			_logger.LogInformation("Start tagging documents in source workspace ArtifactID: {workspaceID}", configuration.DestinationWorkspaceArtifactId);
			List<int> pushedDocumentArtifactIds = (await importJob.GetPushedDocumentArtifactIdsAsync().ConfigureAwait(false)).ToList();
			_logger.LogInformation("Number of pushed documents to tag: {numberOfDocuments}", pushedDocumentArtifactIds.Count);

			TaggingExecutionResult taggingResult =
				await _documentsTagRepository.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(configuration, pushedDocumentArtifactIds, token).ConfigureAwait(false);

			_logger.LogInformation("Documents tagging in source workspace ArtifactID: {workspaceID} Result: {result}", configuration.DestinationWorkspaceArtifactId,
				taggingResult.Status);

			return taggingResult;
		}
	}
}
