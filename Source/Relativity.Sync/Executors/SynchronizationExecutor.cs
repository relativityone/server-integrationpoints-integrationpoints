using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.EDDS.WebAPI.FileManagerBase;
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

			ExecutionResult importAndTagResult = ExecutionResult.Success();
			var destinationTaggingTasks = new List<Task<ExecutionResult>>();
			var sourceTaggingTasks = new List<Task<ExecutionResult>>();
			ExecutionResult[] destinationTaggingResults = { ExecutionResult.Success() };
			ExecutionResult[] sourceTaggingResults = { ExecutionResult.Success() };

			try
			{
				_logger.LogInformation("Gathering batches to execute.");
				IEnumerable<int> batchesIds = await _batchRepository.GetAllNewBatchesIdsAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false);
				Dictionary<int, ImportJobResult> batchJobsResults = new Dictionary<int, ImportJobResult>();

				using (IJobProgressHandler progressHandler = _jobProgressHandlerFactory.CreateJobProgressHandler())
				{
					foreach (int batchId in batchesIds)
					{
						if (token.IsCancellationRequested)
						{
							_logger.LogInformation("Import job has been canceled.");
							batchJobsResults[batchId] = new ImportJobResult(ExecutionResult.Canceled(), 0);
							break;
						}

						_logger.LogInformation("Processing batch ID: {batchId}", batchId);
						IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId)
							.ConfigureAwait(false);
						using (IImportJob importJob = await _importJobFactory.CreateImportJobAsync(configuration, batch, token).ConfigureAwait(false))
						using (progressHandler.AttachToImportJob(importJob.SyncImportBulkArtifactJob, batchId, batch.TotalItemsCount))
						{
							ImportJobResult importJobResult = await importJob.RunAsync(token).ConfigureAwait(false);
							batchJobsResults[batchId] = importJobResult;

							_jobStatisticsContainer.TotalBytesTransferred += importJobResult.JobSizeInBytes;

							IEnumerable<int> pushedDocumentArtifactIds =
								await importJob.GetPushedDocumentArtifactIds().ConfigureAwait(false);
							Task<ExecutionResult> destinationTaggingResult =
								_documentsTagRepository.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(
									configuration, pushedDocumentArtifactIds, token);
							destinationTaggingTasks.Add(destinationTaggingResult);

							IEnumerable<string> pushedDocumentIdentifiers =
								await importJob.GetPushedDocumentIdentifiers().ConfigureAwait(false);
							Task<ExecutionResult> sourceTaggingResult =
								_documentsTagRepository.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(
									configuration, pushedDocumentIdentifiers, token);
							sourceTaggingTasks.Add(sourceTaggingResult);

							if (importJobResult.ExecutionResult.Status == ExecutionStatus.Failed)
							{
								_logger.LogError(importJobResult.ExecutionResult.Exception,
									"Batch ID: {batchId} processing failed with error: {error}", batchId,
									importJobResult.ExecutionResult.Message);
								break;
							}
						}

						_logger.LogInformation("Batch ID: {batchId} processed successfully.", batchId);
					}
				}

				importAndTagResult = AggregateBatchExecutionResults(batchJobsResults);
			}
			catch (ImportFailedException ex)
			{
				const string message = "Fatal exception occurred while executing import job.";
				_logger.LogError(ex, message);
				importAndTagResult = ExecutionResult.Failure(message, ex);
			}
			catch (Exception ex)
			{
				const string message = "Unexpected exception occurred while executing synchronization.";
				_logger.LogError(ex, message);
				importAndTagResult = ExecutionResult.Failure(message, ex);
			}

			try
			{
				destinationTaggingResults =
					await Task.WhenAll(destinationTaggingTasks).ConfigureAwait(false);
				sourceTaggingResults = await Task.WhenAll(sourceTaggingTasks).ConfigureAwait(false);
			}
			catch (OperationCanceledException oce)
			{
				const string taggingCanceledMessage = "Tagging synchronized documents in workspace was interrupted due to the job being canceled.";
				_logger.LogInformation(oce, taggingCanceledMessage);
				importAndTagResult = new ExecutionResult(ExecutionStatus.Canceled, taggingCanceledMessage, oce);
			}
			catch (Exception ex)
			{
				const string message = "Unexpected exception occurred while tagging synchronized documents in workspace.";
				_logger.LogError(ex, message);
				importAndTagResult = ExecutionResult.Failure(message, ex);
			}

			ExecutionResult executionResult = importAndTagResult;

			if (destinationTaggingResults.Any(x => x.Status == ExecutionStatus.Failed) ||
				sourceTaggingResults.Any(x => x.Status == ExecutionStatus.Failed) || token.IsCancellationRequested)
			{
				ExecutionResult destinationTaggingResult = ExecutionResult.Success();
				ExecutionResult sourceTaggingResult = ExecutionResult.Success();

				if (!token.IsCancellationRequested)
				{
					destinationTaggingResult = destinationTaggingResults.First(x => x.Status == ExecutionStatus.Failed);
					sourceTaggingResult = sourceTaggingResults.First(x => x.Status == ExecutionStatus.Failed);
				}

				string[] messages = { executionResult.Message, destinationTaggingResult.Message, sourceTaggingResult.Message };
				string resultMessage = string.Join(" ", messages.Where(m => !string.IsNullOrEmpty(m)));
				Exception[] exceptions = { executionResult.Exception, destinationTaggingResult.Exception, sourceTaggingResult.Exception };
				Exception resultException = new AggregateException(exceptions.Where(e => e != null));
				ExecutionStatus resultStatus = token.IsCancellationRequested ? ExecutionStatus.Canceled : ExecutionStatus.Failed;
				executionResult = new ExecutionResult(resultStatus, resultMessage, resultException);
			}

			_jobCleanupConfiguration.SynchronizationExecutionResult = executionResult;
			return executionResult;
		}

		private ExecutionResult AggregateBatchExecutionResults(Dictionary<int, ImportJobResult> batchJobsResults)
		{
			if (batchJobsResults.Values.Any(x => x.ExecutionResult.Status == ExecutionStatus.Canceled))
			{
				return ExecutionResult.Canceled();
			}

			if (batchJobsResults.Values.Any(x => x.ExecutionResult.Status == ExecutionStatus.Failed))
			{
				return batchJobsResults.Single(x => x.Value.ExecutionResult.Status == ExecutionStatus.Failed).Value.ExecutionResult;
			}

			KeyValuePair<int, ImportJobResult>[] completedWithErrorsBatchJobResults =
				batchJobsResults.Where(x => x.Value.ExecutionResult.Status == ExecutionStatus.CompletedWithErrors).ToArray();
			if (completedWithErrorsBatchJobResults.Any())
			{
				string exceptionMessage = string.Join(Environment.NewLine, completedWithErrorsBatchJobResults.Select(x => $"BatchID: {x.Key} {{x.ExecutionResult.Message}}"));
				AggregateException aggregateException = new AggregateException(
					exceptionMessage,
					completedWithErrorsBatchJobResults.Select(x => x.Value.ExecutionResult.Exception).Where(x => x != null)
					);

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