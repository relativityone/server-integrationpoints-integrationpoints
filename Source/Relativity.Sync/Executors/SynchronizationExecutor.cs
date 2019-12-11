using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	internal sealed class SynchronizationExecutor : IExecutor<ISynchronizationConfiguration>
	{
		private readonly IBatchRepository _batchRepository;
		private readonly IJobProgressHandlerFactory _jobProgressHandlerFactory;
		private readonly IImportJobFactory _importJobFactory;
		private readonly IFieldManager _fieldManager;
		private readonly IFieldMappings _fieldMappings;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly IDocumentTagRepository _documentsTagRepository;
		private readonly IJobCleanupConfiguration _jobCleanupConfiguration;
		private readonly ISyncLog _logger;

		public SynchronizationExecutor(IImportJobFactory importJobFactory,
			IBatchRepository batchRepository,
			IJobProgressHandlerFactory jobProgressHandlerFactory,
			IDocumentTagRepository documentsTagRepository,
			IFieldManager fieldManager,
			IFieldMappings fieldMappings,
			IJobStatisticsContainer jobStatisticsContainer,
			IJobCleanupConfiguration jobCleanupConfiguration,
			ISyncLog logger)
		{
			_batchRepository = batchRepository;
			_jobProgressHandlerFactory = jobProgressHandlerFactory;
			_importJobFactory = importJobFactory;
			_fieldManager = fieldManager;
			_fieldMappings = fieldMappings;
			_jobStatisticsContainer = jobStatisticsContainer;
			_documentsTagRepository = documentsTagRepository;
			_jobCleanupConfiguration = jobCleanupConfiguration;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ISynchronizationConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Creating settings for ImportAPI.");
			UpdateImportSettings(configuration);

			ExecutionResult importAndTagResult = await ExecuteSynchronizationAsync(configuration, token).ConfigureAwait(false);

			_jobCleanupConfiguration.SynchronizationExecutionResult = importAndTagResult;
			return importAndTagResult;
		}

		private async Task<ExecutionResult> ExecuteSynchronizationAsync(ISynchronizationConfiguration configuration,
			CancellationToken token)
		{
			ExecutionResult importAndTagResult;
			try
			{
				_logger.LogInformation("Gathering batches to execute.");
				IEnumerable<int> batchesIds = await _batchRepository
					.GetAllNewBatchesIdsAsync(configuration.SourceWorkspaceArtifactId,
						configuration.SyncConfigurationArtifactId).ConfigureAwait(false);
				Dictionary<int, ExecutionResult> batchesCompletedWithErrors = new Dictionary<int, ExecutionResult>();

				using (IJobProgressHandler progressHandler = _jobProgressHandlerFactory.CreateJobProgressHandler())
				{
					foreach (int batchId in batchesIds)
					{
						if (token.IsCancellationRequested)
						{
							_logger.LogInformation("Import job has been canceled.");
							return ExecutionResult.Canceled();
						}

						_logger.LogInformation("Processing batch ID: {batchId}", batchId);
						IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId)
							.ConfigureAwait(false);
						using (IImportJob importJob = await _importJobFactory.CreateImportJobAsync(configuration, batch, token).ConfigureAwait(false))
						{
							using (progressHandler.AttachToImportJob(importJob.SyncImportBulkArtifactJob, batch.ArtifactId, batch.TotalItemsCount))
							{
								ExecutionResult batchProcessingResult = await ProcessBatchAsync(importJob, batch, progressHandler, token).ConfigureAwait(false);

								Task<ExecutionResult> destinationDocumentsTaggingTask = TagDestinationDocumentsAsync(importJob, configuration, token);
								Task<ExecutionResult> sourceDocumentsTaggingTask = TagSourceDocumentsAsync(importJob, configuration, token);

								ExecutionResult sourceTaggingResult = await sourceDocumentsTaggingTask.ConfigureAwait(false);
								ExecutionResult destinationTaggingResult = await destinationDocumentsTaggingTask.ConfigureAwait(false);

								if (batchProcessingResult.Status == ExecutionStatus.CompletedWithErrors)
								{
									batchesCompletedWithErrors[batch.ArtifactId] = batchProcessingResult;
								}

								ExecutionResult failureResult = AggregateFailuresOrCancelled(batch.ArtifactId,
									batchProcessingResult, sourceTaggingResult, destinationTaggingResult);
								if (failureResult != null)
								{
									return failureResult;
								}
							}
						}

						_logger.LogInformation("Batch ID: {batchId} processed successfully.", batch.ArtifactId);
					}

					importAndTagResult = AggregateBatchesCompletedWithErrorsResults(batchesCompletedWithErrors);
				}
			}
			catch (ImportFailedException ex)
			{
				const string message = "Fatal exception occurred while executing import job.";
				_logger.LogError(ex, message);
				importAndTagResult = ExecutionResult.Failure(message, ex);
			}
			catch (OperationCanceledException oce)
			{
				const string taggingCanceledMessage = "Executing synchronization was interrupted due to the job being canceled.";
				_logger.LogInformation(oce, taggingCanceledMessage);
				importAndTagResult = new ExecutionResult(ExecutionStatus.Canceled, taggingCanceledMessage, oce);
			}
			catch (Exception ex)
			{
				const string message = "Unexpected exception occurred while executing synchronization.";
				_logger.LogError(ex, message);
				importAndTagResult = ExecutionResult.Failure(message, ex);
			}

			return importAndTagResult;
		}

		private static ExecutionResult AggregateFailuresOrCancelled(int batchId, params ExecutionResult[] executionResults)
		{
			List<ExecutionResult> failedResults = executionResults.Where(x => x.Status == ExecutionStatus.Failed).ToList();

			if (failedResults.Any())
			{
				string message = $"Processing batch (id: {batchId}) failed: {string.Join(";", failedResults.Select(x => x.Message))}";
				Exception exception = new AggregateException(failedResults.Select(x => x.Exception));
				return ExecutionResult.Failure(message, exception);
			}

			if (executionResults.Any(x => x.Status == ExecutionStatus.Canceled))
			{
				return ExecutionResult.Canceled();
			}

			return null;
		}

		private async Task<ExecutionResult> ProcessBatchAsync(IImportJob importJob, IBatch batch, IJobProgressHandler progressHandler, CancellationToken token)
		{
			ExecutionResult processBatchResult = await RunImportJobAsync(importJob, token).ConfigureAwait(false);

			int failedItemsCount = progressHandler.GetBatchItemsFailedCount(batch.ArtifactId);
			await batch.SetFailedItemsCountAsync(failedItemsCount).ConfigureAwait(false);

			int processedItemsCount = progressHandler.GetBatchItemsProcessedCount(batch.ArtifactId);
			await batch.SetTransferredItemsCountAsync(processedItemsCount).ConfigureAwait(false);

			return processBatchResult;
		}

		private async Task<ExecutionResult> RunImportJobAsync(IImportJob importJob, CancellationToken token)
		{
			ImportJobResult importJobResult = await importJob.RunAsync(token).ConfigureAwait(false);

			_jobStatisticsContainer.TotalBytesTransferred += importJobResult.JobSizeInBytes;

			return importJobResult.ExecutionResult;
		}

		private async Task<ExecutionResult> TagDestinationDocumentsAsync(IImportJob importJob, ISynchronizationConfiguration configuration,
			CancellationToken token)
		{
			IEnumerable<string> pushedDocumentIdentifiers =
				await importJob.GetPushedDocumentIdentifiersAsync().ConfigureAwait(false);
			ExecutionResult sourceTaggingResult =
				await _documentsTagRepository.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(
					configuration, pushedDocumentIdentifiers, token).ConfigureAwait(false);

			return sourceTaggingResult;
		}

		private async Task<ExecutionResult> TagSourceDocumentsAsync(IImportJob importJob, ISynchronizationConfiguration configuration,
			CancellationToken token)
		{
			IEnumerable<int> pushedDocumentArtifactIds =
				await importJob.GetPushedDocumentArtifactIdsAsync().ConfigureAwait(false);
			ExecutionResult destinationTaggingResult =
				await _documentsTagRepository.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(
					configuration, pushedDocumentArtifactIds, token).ConfigureAwait(false);

			return destinationTaggingResult;
		}

		private static ExecutionResult AggregateBatchesCompletedWithErrorsResults(Dictionary<int, ExecutionResult> batchesCompletedWithErrorsResults)
		{
			if (batchesCompletedWithErrorsResults.Any())
			{
				string exceptionMessage = string.Join(Environment.NewLine, batchesCompletedWithErrorsResults.Select(x => $"BatchID: {x.Key} {x.Value.Message}"));
				AggregateException aggregateException = new AggregateException(exceptionMessage,
					batchesCompletedWithErrorsResults.Select(x => x.Value.Exception).Where(x => x != null));

				return ExecutionResult.SuccessWithErrors(aggregateException);
			}

			return ExecutionResult.Success();
		}

		private void UpdateImportSettings(ISynchronizationConfiguration configuration)
		{
			int destinationIdentityFieldId = GetDestinationIdentityFieldId(_fieldMappings.GetFieldMappings());
			IList<FieldInfoDto> specialFields = _fieldManager.GetSpecialFields().ToList();

			configuration.IdentityFieldId = destinationIdentityFieldId;
			if (configuration.DestinationFolderStructureBehavior != DestinationFolderStructureBehavior.None)
			{
				configuration.FolderPathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.FolderPath);
			}
			configuration.FileSizeColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileSize);
			configuration.NativeFilePathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileLocation);
			configuration.FileNameColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileFilename);
			configuration.OiFileTypeColumnName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.RelativityNativeType);
			configuration.SupportedByViewerColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.SupportedByViewer);
		}

		private int GetDestinationIdentityFieldId(IList<FieldMap> fieldMappings)
		{
			FieldMap destinationIdentityField = fieldMappings.FirstOrDefault(x => x.DestinationField.IsIdentifier);
			if (destinationIdentityField == null)
			{
				const string message = "Cannot find destination identifier field in field mappings.";
				_logger.LogError(message);
				throw new SyncException(message);
			}
			return destinationIdentityField.DestinationField.FieldIdentifier;
		}

		private string GetSpecialFieldColumnName(IList<FieldInfoDto> specialFields, SpecialFieldType specialFieldType)
		{
			FieldInfoDto specialField = specialFields.FirstOrDefault(x => x.SpecialFieldType == specialFieldType);

			if (specialField == null)
			{
				string message = $"Cannot find special field name: {specialFieldType}";
				_logger.LogError(message);
				throw new SyncException(message);
			}
			return specialField.DestinationFieldName;
		}
	}
}