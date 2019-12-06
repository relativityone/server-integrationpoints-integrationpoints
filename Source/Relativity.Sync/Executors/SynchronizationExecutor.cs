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
		private readonly IJobProgressUpdaterFactory _jobProgressUpdaterFactory;
		private readonly IBatchRepository _batchRepository;
		private readonly IJobProgressHandlerFactory _jobProgressHandlerFactory;
		private readonly IImportJobFactory _importJobFactory;
		private readonly IFieldManager _fieldManager;
		private readonly IFieldMappings _fieldMappings;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly IDocumentTagRepository _documentsTagRepository;
		private readonly IJobCleanupConfiguration _jobCleanupConfiguration;
		private readonly ISyncLog _logger;

		public SynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository,
			IJobProgressHandlerFactory jobProgressHandlerFactory, IJobProgressUpdaterFactory jobProgressUpdaterFactory,
			IDocumentTagRepository documentsTagRepository, IFieldManager fieldManager, IFieldMappings fieldMappings,
			IJobStatisticsContainer jobStatisticsContainer, IJobCleanupConfiguration jobCleanupConfiguration, ISyncLog logger)
		{
			_batchRepository = batchRepository;
			_jobProgressHandlerFactory = jobProgressHandlerFactory;
			_jobProgressUpdaterFactory = jobProgressUpdaterFactory;
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

		public async Task<ExecutionResult> ExecuteSynchronizationAsync(ISynchronizationConfiguration configuration,
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
						using (IImportJob importJob = await _importJobFactory
							.CreateImportJobAsync(configuration, batch, token).ConfigureAwait(false))
						using (progressHandler.AttachToImportJob(importJob.SyncImportBulkArtifactJob, batchId,
							batch.TotalItemsCount))
						{
							var context = new BatchProcessingContext(importJob, batchId, configuration);

							ExecutionResultContext batchProcessingResultContext =
								await ProcessBatchAndHandleResult(context, batchesCompletedWithErrors, token).ConfigureAwait(false);
							if (batchProcessingResultContext.ShouldReturn)
							{
								return batchProcessingResultContext.Result;
							}

							ExecutionResultContext sourceTaggingResultContext =
								await TagSourceDocumentsAndHandleResult(context, token).ConfigureAwait(false);
							if (sourceTaggingResultContext.ShouldReturn)
							{
								return sourceTaggingResultContext.Result;
							}

							ExecutionResultContext destinationTaggingResultContext =
								await TagDestinationDocumentsAndHandleResult(context, token).ConfigureAwait(false);
							if (destinationTaggingResultContext.ShouldReturn)
							{
								return destinationTaggingResultContext.Result;
							}
						}

						_logger.LogInformation("Batch ID: {batchId} processed successfully.", batchId);
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

		private struct BatchProcessingContext
		{
			public readonly IImportJob ImportJob;
			public readonly int BatchId;
			public readonly ISynchronizationConfiguration Configuration;

			public BatchProcessingContext(IImportJob importJob, int batchId, ISynchronizationConfiguration configuration)
			{
				ImportJob = importJob;
				BatchId = batchId;
				Configuration = configuration;
			}
		}

		private struct ExecutionResultContext
		{
			public readonly bool ShouldReturn;
			public readonly ExecutionResult Result;

			public ExecutionResultContext(bool shouldReturn, ExecutionResult result)
			{
				ShouldReturn = shouldReturn;
				Result = result;
			}
		}

		private async Task<ExecutionResultContext> ProcessBatchAndHandleResult(BatchProcessingContext context, IDictionary<int, ExecutionResult> batchesCompletedWithErrors, CancellationToken token)
		{
			ExecutionResult processBatchResult = await BatchProcessing(context.ImportJob, token).ConfigureAwait(false);

			if (processBatchResult.Status == ExecutionStatus.CompletedWithErrors)
			{
				batchesCompletedWithErrors[context.BatchId] = processBatchResult;
			}

			return HandleExecutionResult(processBatchResult, "processing", context.BatchId);
		}

		private async Task<ExecutionResultContext> TagSourceDocumentsAndHandleResult(BatchProcessingContext context, CancellationToken token)
		{
			ExecutionResult sourceDocumentsTaggingResult = await SourceDocumentsTagging(context.Configuration, context.ImportJob, token).ConfigureAwait(false);

			return HandleExecutionResult(sourceDocumentsTaggingResult, "source tagging", context.BatchId);

		}

		private async Task<ExecutionResultContext> TagDestinationDocumentsAndHandleResult(BatchProcessingContext context, CancellationToken token)
		{
			ExecutionResult destinationDocumentsTaggingResult = await DestinationDocumentsTagging(context.Configuration, context.ImportJob, token).ConfigureAwait(false);

			return HandleExecutionResult(destinationDocumentsTaggingResult, "destination tagging", context.BatchId);
		}

		private ExecutionResultContext HandleExecutionResult(ExecutionResult result, string actionName, int batchId)
		{
			switch (result.Status)
			{
				case ExecutionStatus.Canceled:
					return new ExecutionResultContext(true, result);
				case ExecutionStatus.Failed:
					_logger.LogError(result.Exception,
						$"Batch ID: {{batchId}} {actionName} failed with error: {{error}}",
						batchId, result.Message);

					return new ExecutionResultContext(true, result);
			}

			return new ExecutionResultContext(false, result);
		}

		private async Task<ExecutionResult> BatchProcessing(IImportJob importJob, CancellationToken token)
		{
			ImportJobResult importJobResult = await importJob.RunAsync(token).ConfigureAwait(false);

			_jobStatisticsContainer.TotalBytesTransferred += importJobResult.JobSizeInBytes;

			return importJobResult.ExecutionResult;
		}

		private async Task<ExecutionResult> DestinationDocumentsTagging(ISynchronizationConfiguration configuration, IImportJob importJob,
			CancellationToken token)
		{
			IEnumerable<string> pushedDocumentIdentifiers =
				await importJob.GetPushedDocumentIdentifiersAsync().ConfigureAwait(false);
			ExecutionResult sourceTaggingResult =
				await _documentsTagRepository.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(
					configuration, pushedDocumentIdentifiers, token).ConfigureAwait(false);

			return sourceTaggingResult;
		}

		private async Task<ExecutionResult> SourceDocumentsTagging(ISynchronizationConfiguration configuration, IImportJob importJob,
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