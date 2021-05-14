using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Extensions;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors
{
	internal abstract class SynchronizationExecutorBase<TConfiguration> : IExecutor<TConfiguration>
		where TConfiguration : ISynchronizationConfiguration
	{
		private readonly IBatchRepository _batchRepository;
		private readonly IJobProgressHandlerFactory _jobProgressHandlerFactory;
		private readonly IFieldMappings _fieldMappings;
		private readonly IDocumentTagRepository _documentsTagRepository;
		private readonly IJobCleanupConfiguration _jobCleanupConfiguration;
		private readonly IAutomatedWorkflowTriggerConfiguration _automatedWorkflowTriggerConfiguration;
		private readonly Func<IStopwatch> _stopwatchFactory;

		protected readonly ISyncMetrics _syncMetrics;
		private readonly IUserContextConfiguration _userContextConfiguration;
		protected readonly IJobStatisticsContainer _jobStatisticsContainer;
		protected readonly IImportJobFactory _importJobFactory;
		protected readonly BatchRecordType _recordType;
		protected readonly IFieldManager _fieldManager;
		protected readonly ISyncLog _logger;

		protected SynchronizationExecutorBase(IImportJobFactory importJobFactory,
			BatchRecordType recordType,
			IBatchRepository batchRepository,
			IJobProgressHandlerFactory jobProgressHandlerFactory,
			IDocumentTagRepository documentsTagRepository,
			IFieldManager fieldManager,
			IFieldMappings fieldMappings,
			IJobStatisticsContainer jobStatisticsContainer,
			IJobCleanupConfiguration jobCleanupConfiguration,
			IAutomatedWorkflowTriggerConfiguration automatedWorkflowTriggerConfiguration,
			Func<IStopwatch> stopwatchFactory,
			ISyncMetrics syncMetrics,
			IUserContextConfiguration userContextConfiguration,
			ISyncLog logger)
		{
			_batchRepository = batchRepository;
			_jobProgressHandlerFactory = jobProgressHandlerFactory;
			_importJobFactory = importJobFactory;
			_recordType = recordType;
			_fieldManager = fieldManager;
			_fieldMappings = fieldMappings;
			_jobStatisticsContainer = jobStatisticsContainer;
			_documentsTagRepository = documentsTagRepository;
			_jobCleanupConfiguration = jobCleanupConfiguration;
			_automatedWorkflowTriggerConfiguration = automatedWorkflowTriggerConfiguration;
			_stopwatchFactory = stopwatchFactory;
			_syncMetrics = syncMetrics;
			_userContextConfiguration = userContextConfiguration;
			_logger = logger;
		}

		protected abstract Task<IImportJob> CreateImportJobAsync(TConfiguration configuration, IBatch batch, CancellationToken token);

		protected abstract void UpdateImportSettings(TConfiguration configuration);

		protected abstract void ChildReportBatchMetrics(int batchId, BatchProcessResult batchProcessResult, TimeSpan batchTime, TimeSpan importApiTimer);
		
		protected void ReportBatchMetrics(int batchId, int savedSearchId, BatchProcessResult batchProcessResult, TimeSpan batchTime,
			TimeSpan importApiTimer)
		{
			_syncMetrics.Send(GetBatchPerformanceMetric(batchId, savedSearchId, batchProcessResult, importApiTimer));
			ChildReportBatchMetrics(batchId, batchProcessResult, batchTime, importApiTimer);
		}

		private IMetric GetBatchPerformanceMetric(int batchId, int savedSearchId, BatchProcessResult batchProcessResult, TimeSpan importApiTimer)
		{
			var metric = new BatchEndPerformanceMetric
			{
				Elapsed = (long) importApiTimer.TotalSeconds,
				JobID = batchId,
				WorkspaceID = _jobCleanupConfiguration.SourceWorkspaceArtifactId,
				RecordNumber = batchProcessResult.TotalRecordsTransferred,
				JobSizeGB = UnitsConverter.BytesToGigabytes(batchProcessResult.BytesTransferred),
				JobSizeGB_Metadata = UnitsConverter.BytesToGigabytes(batchProcessResult.MetadataBytesTransferred),
				JobSizeGB_Files = UnitsConverter.BytesToGigabytes(batchProcessResult.FilesBytesTransferred),
				UserID = _userContextConfiguration.ExecutingUserId,
				SavedSearchID = savedSearchId,
				RecordType = _recordType,
				JobStatus = batchProcessResult.ExecutionResult.Status
			};

			return metric;
		}

		public async Task<ExecutionResult> ExecuteAsync(TConfiguration configuration, CompositeCancellationToken token)
		{
			_logger.LogInformation("Creating settings for ImportAPI.");
			UpdateImportSettings(configuration);

			ExecutionResult importAndTagResult = await ExecuteSynchronizationAsync(configuration, token).ConfigureAwait(false);

			_jobCleanupConfiguration.SynchronizationExecutionResult = importAndTagResult;
			_automatedWorkflowTriggerConfiguration.SynchronizationExecutionResult = importAndTagResult;
			return importAndTagResult;
		}

		private async Task<ExecutionResult> ExecuteSynchronizationAsync(TConfiguration configuration, CompositeCancellationToken token)
		{
			ExecutionResult importAndTagResult;
			try
			{
				_logger.LogInformation("Gathering batches to execute.");
				IEnumerable<int> batchesIds = await _batchRepository
					.GetAllBatchesIdsToExecuteAsync(configuration.SourceWorkspaceArtifactId,
						configuration.SyncConfigurationArtifactId).ConfigureAwait(false);
				Dictionary<int, ExecutionResult> batchesCompletedWithErrors = new Dictionary<int, ExecutionResult>();

				using (IJobProgressHandler progressHandler = _jobProgressHandlerFactory.CreateJobProgressHandler())
				{
					foreach (int batchId in batchesIds)
					{
						if (token.StopCancellationToken.IsCancellationRequested)
						{
							_logger.LogInformation("Import job has been canceled.");
							return ExecutionResult.Canceled();
						}

						_logger.LogInformation("Processing batch ID: {batchId}", batchId);
						IStopwatch batchTimer = GetStartedTimer();
						IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId).ConfigureAwait(false);
						using (IImportJob importJob = await CreateImportJobAsync(configuration, batch, token.AnyReasonCancellationToken).ConfigureAwait(false))
						{
							using (progressHandler.AttachToImportJob(importJob.SyncImportBulkArtifactJob, batch.ArtifactId, batch.TotalItemsCount))
							{
								IStopwatch importApiTimer = GetStartedTimer();
								BatchProcessResult batchProcessingResult = await ProcessBatchAsync(importJob, batch, progressHandler, token).ConfigureAwait(false);
								importApiTimer.Stop();

								Task<TaggingExecutionResult> destinationDocumentsTaggingTask = TagDestinationDocumentsAsync(importJob, configuration, token.StopCancellationToken);
								Task<TaggingExecutionResult> sourceDocumentsTaggingTask = TagSourceDocumentsAsync(importJob, configuration, token.StopCancellationToken);
								
								TaggingExecutionResult sourceTaggingResult = await sourceDocumentsTaggingTask.ConfigureAwait(false);
								TaggingExecutionResult destinationTaggingResult = await destinationDocumentsTaggingTask.ConfigureAwait(false);

								int documentsTaggedCount = Math.Min(sourceTaggingResult.TaggedDocumentsCount, destinationTaggingResult.TaggedDocumentsCount);
								await batch.SetTaggedItemsCountAsync(batch.TaggedItemsCount + documentsTaggedCount).ConfigureAwait(false);
								batchProcessingResult.TotalRecordsTagged = documentsTaggedCount;

								if (batchProcessingResult.ExecutionResult.Status == ExecutionStatus.CompletedWithErrors)
								{
									batchesCompletedWithErrors[batch.ArtifactId] = batchProcessingResult.ExecutionResult;
								}
								
								batchTimer.Stop();
								ReportBatchMetrics(batchId, configuration.DataSourceArtifactId, batchProcessingResult, batchTimer.Elapsed, importApiTimer.Elapsed);

								ExecutionResult failureResult = AggregateFailuresOrCancelled(batch.ArtifactId,
									batchProcessingResult.ExecutionResult, sourceTaggingResult, destinationTaggingResult);
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

			if (executionResults.Any(x => x.Status == ExecutionStatus.Paused))
			{
				return ExecutionResult.Paused();
			}

			return null;
		}

		private async Task<BatchProcessResult> ProcessBatchAsync(IImportJob importJob, IBatch batch, IJobProgressHandler progressHandler, CompositeCancellationToken token)
		{
			BatchProcessResult batchProcessResult = await RunImportJobAsync(importJob, token).ConfigureAwait(false);
		
			int failedItemsCount = progressHandler.GetBatchItemsFailedCount(batch.ArtifactId);
			await batch.SetFailedItemsCountAsync(batch.FailedItemsCount + failedItemsCount).ConfigureAwait(false);

			int processedItemsCount = progressHandler.GetBatchItemsProcessedCount(batch.ArtifactId);
			await batch.SetTransferredItemsCountAsync(batch.TransferredItemsCount + processedItemsCount).ConfigureAwait(false);

			if (batchProcessResult.ExecutionResult.Status == ExecutionStatus.Paused)
			{
				batchProcessResult.ExecutionResult = await HandleBatchPausedAsync(batch).ConfigureAwait(false);
			}
			else
			{
				await SetBatchStatusAsync(batch, batchProcessResult.ExecutionResult.Status).ConfigureAwait(false);
			}

			batchProcessResult.TotalRecordsRequested = batch.TotalItemsCount;
			batchProcessResult.TotalRecordsTransferred = batch.TransferredItemsCount;
			batchProcessResult.TotalRecordsFailed = batch.FailedItemsCount;

			return batchProcessResult;
		}

		private async Task<ExecutionResult> HandleBatchPausedAsync(IBatch batch)
		{
			if(batch.TransferredItemsCount == batch.TotalItemsCount)
			{
				await SetBatchStatusAsync(batch, ExecutionStatus.Completed).ConfigureAwait(false);
				return ExecutionResult.Success();
			}
			else if(batch.TransferredItemsCount + batch.FailedItemsCount == batch.TotalItemsCount)
			{
				await SetBatchStatusAsync(batch, ExecutionStatus.CompletedWithErrors).ConfigureAwait(false);
				return ExecutionResult.SuccessWithErrors();
			}
			else
			{
				await batch.SetStartingIndexAsync(batch.TransferredItemsCount + batch.FailedItemsCount).ConfigureAwait(false);
				await SetBatchStatusAsync(batch, ExecutionStatus.Paused).ConfigureAwait(false);
				return ExecutionResult.Paused();
			}
		}

		private Task SetBatchStatusAsync(IBatch batch, ExecutionStatus executionResultStatus)
		{
			BatchStatus status = executionResultStatus.ToBatchStatus();

			_logger.LogInformation("Setting status {status} for batch {batchId}", status, batch.ArtifactId);
			return batch.SetStatusAsync(status);
		}

		private async Task<BatchProcessResult> RunImportJobAsync(IImportJob importJob, CompositeCancellationToken token)
		{
			ImportJobResult importJobResult = await importJob.RunAsync(token).ConfigureAwait(false);

			_jobStatisticsContainer.MetadataBytesTransferred += importJobResult.MetadataSizeInBytes;
			_jobStatisticsContainer.FilesBytesTransferred += importJobResult.FilesSizeInBytes;
			_jobStatisticsContainer.TotalBytesTransferred += importJobResult.JobSizeInBytes;

			return new BatchProcessResult()
			{
				ExecutionResult = importJobResult.ExecutionResult,
				FilesBytesTransferred = importJobResult.FilesSizeInBytes,
				MetadataBytesTransferred = importJobResult.MetadataSizeInBytes,
				BytesTransferred = importJobResult.JobSizeInBytes
			};
		}

		private async Task<TaggingExecutionResult> TagDestinationDocumentsAsync(IImportJob importJob,
			ISynchronizationConfiguration configuration,
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

		private async Task<TaggingExecutionResult> TagSourceDocumentsAsync(IImportJob importJob,
			ISynchronizationConfiguration configuration,
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

		protected int GetDestinationIdentityFieldId()
		{
			FieldMap destinationIdentityField = _fieldMappings.GetFieldMappings().FirstOrDefault(x => x.DestinationField.IsIdentifier);
			if (destinationIdentityField == null)
			{
				const string message = "Cannot find destination identifier field in field mappings.";
				_logger.LogError(message);
				throw new SyncException(message);
			}
			return destinationIdentityField.DestinationField.FieldIdentifier;
		}

		protected string GetSpecialFieldColumnName(IList<FieldInfoDto> specialFields, SpecialFieldType specialFieldType)
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

		private IStopwatch GetStartedTimer()
		{
			IStopwatch timer = _stopwatchFactory();
			timer.Start();
			return timer;
		}

		protected class BatchProcessResult
		{
			public ExecutionResult ExecutionResult { get; set; }
			public int TotalRecordsRequested { get; set; }
			public int TotalRecordsTransferred { get; set; }
			public int TotalRecordsFailed { get; set; }
			public int TotalRecordsTagged { get; set; }
			public long MetadataBytesTransferred { get; set; }
			public long FilesBytesTransferred { get; set; }
			public long BytesTransferred { get; set; }
		}
	}
}