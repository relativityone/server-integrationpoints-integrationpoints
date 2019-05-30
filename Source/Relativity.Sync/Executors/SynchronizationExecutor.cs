using System;
using System.Collections.Generic;
using System.Globalization;
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
		private readonly IDateTime _dateTime;
		private readonly IDestinationWorkspaceTagRepository _destinationWorkspaceTagRepository;
		private readonly IImportJobFactory _importJobFactory;
		private readonly IFieldManager _fieldManager;
		private readonly IFieldMappings _fieldMappings;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly ISyncLog _logger;
		private readonly ISyncMetrics _syncMetrics;

		public SynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository, IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository,
			ISyncMetrics syncMetrics, IDateTime dateTime, IFieldManager fieldManager, IFieldMappings fieldMappings, IJobHistoryErrorRepository jobHistoryErrorRepository, ISyncLog logger)
		{
			_batchRepository = batchRepository;
			_dateTime = dateTime;
			_destinationWorkspaceTagRepository = destinationWorkspaceTagRepository;
			_importJobFactory = importJobFactory;
			_syncMetrics = syncMetrics;
			_fieldManager = fieldManager;
			_fieldMappings = fieldMappings;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ISynchronizationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Creating settings for ImportAPI.");
			UpdateImportSettings(configuration);

			ExecutionResult importResult = ExecutionResult.Success();
			DateTime startTime = _dateTime.Now;
			var taggingTasks = new List<Task<IEnumerable<int>>>();
			try
			{
				_logger.LogVerbose("Gathering batches to execute.");
				IEnumerable<int> batchesIds = await _batchRepository.GetAllNewBatchesIdsAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false);

				foreach (int batchId in batchesIds)
				{
					if (token.IsCancellationRequested)
					{
						_logger.LogInformation("Import job has been canceled.");
						importResult = ExecutionResult.Canceled();
						break;
					}

					_logger.LogVerbose("Processing batch ID: {batchId}", batchId);
					IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId).ConfigureAwait(false);
					using (IImportJob importJob = await _importJobFactory.CreateImportJobAsync(configuration, batch).ConfigureAwait(false))
					{
						importResult = await importJob.RunAsync(token).ConfigureAwait(false);

						IEnumerable<int> pushedDocumentArtifactIds = await importJob.GetPushedDocumentArtifactIds().ConfigureAwait(false);
						Task<IEnumerable<int>> taggingTask = TagDocumentsAsync(configuration, pushedDocumentArtifactIds, token);
						taggingTasks.Add(taggingTask);
					}
					_logger.LogInformation("Batch ID: {batchId} processed successfully.", batchId);
				}
			}
			catch (ImportFailedException ex)
			{
				const string message = "Fatal exception occurred while executing import job.";
				_logger.LogError(ex, message);
				importResult = ExecutionResult.Failure(message, ex);
			}
			catch (Exception ex)
			{
				const string message = "Unexpected exception occurred while executing synchronization.";
				_logger.LogError(ex, message);
				importResult = ExecutionResult.Failure(message, ex);
			}
			finally
			{
				DateTime endTime = _dateTime.Now;
				TimeSpan jobDuration = endTime - startTime;
				_syncMetrics.CountOperation("ImportJobStatus", importResult.Status);
				_syncMetrics.TimedOperation("ImportJob", jobDuration, importResult.Status);
				_syncMetrics.GaugeOperation("ImportJobStart", importResult.Status, startTime.Ticks, "Ticks", new Dictionary<string, object>());
				_syncMetrics.GaugeOperation("ImportJobEnd", importResult.Status, endTime.Ticks, "Ticks", new Dictionary<string, object>());
			}

			ExecutionResult taggingResult = await GetTaggingResults(taggingTasks, configuration.JobHistoryArtifactId).ConfigureAwait(false);
			if (taggingResult.Status == ExecutionStatus.Failed)
			{
				var jobHistoryError = new CreateJobHistoryErrorDto(configuration.JobHistoryArtifactId, ErrorType.Job)
				{
					ErrorMessage = taggingResult.Message,
					StackTrace = taggingResult.Exception?.StackTrace
				};
				await _jobHistoryErrorRepository.CreateAsync(configuration.SourceWorkspaceArtifactId, jobHistoryError).ConfigureAwait(false);
			}

			ExecutionResult executionResult = importResult;
			if (taggingResult.Status == ExecutionStatus.Failed || taggingResult.Status == ExecutionStatus.Canceled)
			{
				string resultMessage = string.IsNullOrEmpty(executionResult.Message) ? taggingResult.Message : string.Join(" ", executionResult.Message, taggingResult.Message);
				Exception resultException = executionResult.Exception == null ? taggingResult.Exception : new AggregateException(executionResult.Exception, taggingResult.Exception);
				executionResult = new ExecutionResult(taggingResult.Status, resultMessage, resultException);
			}
			return executionResult;
		}

		private void UpdateImportSettings(ISynchronizationConfiguration configuration)
		{
			int destinationIdentityFieldId = GetDestinationIdentityFieldId(_fieldMappings.GetFieldMappings());
			IList<FieldInfoDto> specialFields = _fieldManager.GetSpecialFields().ToList();

			configuration.ImportSettings.IdentityFieldId = destinationIdentityFieldId;
			if (configuration.DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.ReadFromField)
			{
				configuration.ImportSettings.FolderPathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.FolderPath);
			}
			configuration.ImportSettings.FileSizeColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileSize);
			configuration.ImportSettings.NativeFilePathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileLocation);
			configuration.ImportSettings.FileNameColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileFilename);
			configuration.ImportSettings.OiFileTypeColumnName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.RelativityNativeType);
			configuration.ImportSettings.SupportedByViewerColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.SupportedByViewer);
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
			return specialField.DisplayName;
		}

		private async Task<IEnumerable<int>> TagDocumentsAsync(ISynchronizationConfiguration configuration, IEnumerable<int> artifactIds, CancellationToken token)
		{
			var failedArtifactIds = new List<int>();
			IList<int> artifactIdsList = artifactIds.ToList();
			if (artifactIdsList.Count > 0)
			{
				IList<TagDocumentsResult> taggingResults = await _destinationWorkspaceTagRepository.TagDocumentsAsync(configuration, artifactIdsList, token).ConfigureAwait(false);
				foreach (TagDocumentsResult taggingResult in taggingResults)
				{
					if (taggingResult.FailedDocumentArtifactIds.Any())
					{
						failedArtifactIds.AddRange(taggingResult.FailedDocumentArtifactIds);
					}
				}
			}
			return failedArtifactIds;
		}

		private async Task<ExecutionResult> GetTaggingResults(IList<Task<IEnumerable<int>>> taggingTasks, int jobHistoryArtifactId)
		{
			ExecutionResult taggingResult = ExecutionResult.Success();
			var failedTagArtifactIds = new List<int>();
			try
			{
				await Task.WhenAll(taggingTasks).ConfigureAwait(false);
				foreach (Task<IEnumerable<int>> task in taggingTasks)
				{
					failedTagArtifactIds.AddRange(task.Result);
				}

				if (failedTagArtifactIds.Any())
				{
					const int maxSubset = 50;
					int subsetCount = failedTagArtifactIds.Count < maxSubset ? failedTagArtifactIds.Count : maxSubset;
					string subsetArtifactIds = string.Join(",", failedTagArtifactIds.Take(subsetCount));

					string errorMessage = $"Failed to tag synchronized documents in source workspace. The first {maxSubset} out of {failedTagArtifactIds.Count} are: {subsetArtifactIds}.";
					var failedTaggingException = new SyncException(errorMessage, jobHistoryArtifactId.ToString(CultureInfo.InvariantCulture));
					taggingResult = ExecutionResult.Failure(errorMessage, failedTaggingException);
				}
			}
			catch (OperationCanceledException oce)
			{
				const string taggingCanceledMessage = "Tagging synchronized documents in source workspace was interrupted due to the job being canceled.";
				_logger.LogInformation(oce, taggingCanceledMessage);
				taggingResult = new ExecutionResult(ExecutionStatus.Canceled, taggingCanceledMessage, oce);
			}
			catch (Exception ex)
			{
				const string message = "Unexpected exception occurred while tagging synchronized documents in source workspace.";
				_logger.LogError(ex, message);
				taggingResult = ExecutionResult.Failure(message, ex);
			}
			return taggingResult;
		}
	}
}