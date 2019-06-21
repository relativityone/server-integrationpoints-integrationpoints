using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	internal sealed class SynchronizationExecutor : IExecutor<ISynchronizationConfiguration>
	{
		private readonly IBatchRepository _batchRepository;
		private readonly IDestinationWorkspaceTagRepository _destinationWorkspaceTagRepository;
		private readonly IImportJobFactory _importJobFactory;
		private readonly IFieldManager _fieldManager;
		private readonly IFieldMappings _fieldMappings;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly ISyncLog _logger;
		private readonly ISourceWorkspaceTagRepository _sourceWorkspaceTagRepository;

		public SynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository, IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository,
			ISourceWorkspaceTagRepository sourceWorkspaceTagRepository, IFieldManager fieldManager, IFieldMappings fieldMappings, IJobHistoryErrorRepository jobHistoryErrorRepository, ISyncLog logger)
		{
			_batchRepository = batchRepository;
			_destinationWorkspaceTagRepository = destinationWorkspaceTagRepository;
			_importJobFactory = importJobFactory;
			_fieldManager = fieldManager;
			_fieldMappings = fieldMappings;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
			_logger = logger;
			_sourceWorkspaceTagRepository = sourceWorkspaceTagRepository;
		}

		public async Task<ExecutionResult> ExecuteAsync(ISynchronizationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Creating settings for ImportAPI.");
			UpdateImportSettings(configuration);

			ExecutionResult importResult = ExecutionResult.Success();
			var destinationTaggingTasks = new List<Task<IEnumerable<int>>>();
			var sourceTaggingTasks = new List<Task<IEnumerable<string>>>();
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
						Task<IEnumerable<int>> destinationTaggingTask = TagDestinationDocumentsAsync(configuration, pushedDocumentArtifactIds, token);
						destinationTaggingTasks.Add(destinationTaggingTask);

						IEnumerable<string> pushedDocumentIdentifiers = await importJob.GetPushedDocumentIdentifiers().ConfigureAwait(false);
						Task<IEnumerable<string>> sourceTaggingTask = TagSourceDocumentsAsync(configuration, pushedDocumentIdentifiers, token);
						sourceTaggingTasks.Add(sourceTaggingTask);
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

			ExecutionResult destinationTaggingResult = await GetTaggingResults(destinationTaggingTasks, configuration.JobHistoryArtifactId).ConfigureAwait(false);
			if (destinationTaggingResult.Status == ExecutionStatus.Failed)
			{
				await GenerateDocumentTaggingJobHistoryError(destinationTaggingResult, configuration).ConfigureAwait(false);
			}
			ExecutionResult sourceTaggingResult = await GetTaggingResults(sourceTaggingTasks, configuration.JobHistoryArtifactId).ConfigureAwait(false);
			if (sourceTaggingResult.Status == ExecutionStatus.Failed)
			{
				await GenerateDocumentTaggingJobHistoryError(sourceTaggingResult, configuration).ConfigureAwait(false);
			}

			ExecutionResult executionResult = importResult;
			if (destinationTaggingResult.Status == ExecutionStatus.Failed || sourceTaggingResult.Status == ExecutionStatus.Failed || token.IsCancellationRequested)
			{
				string[] messages = { executionResult.Message, destinationTaggingResult.Message, sourceTaggingResult.Message };
				string resultMessage = string.Join(" ", messages.Where(m => !string.IsNullOrEmpty(m)));
				Exception[] exceptions = { executionResult.Exception, destinationTaggingResult.Exception, sourceTaggingResult.Exception };
				Exception resultException = new AggregateException(exceptions.Where(e => e != null));
				ExecutionStatus resultStatus = token.IsCancellationRequested ? ExecutionStatus.Canceled : ExecutionStatus.Failed;
				executionResult = new ExecutionResult(resultStatus, resultMessage, resultException);
			}
			return executionResult;
		}

		private async Task GenerateDocumentTaggingJobHistoryError(ExecutionResult taggingResult, ISynchronizationConfiguration configuration)
		{
			var jobHistoryError = new CreateJobHistoryErrorDto(configuration.JobHistoryArtifactId, ErrorType.Job)
			{
				ErrorMessage = taggingResult.Message,
				StackTrace = taggingResult.Exception?.StackTrace
			};
			await _jobHistoryErrorRepository.CreateAsync(configuration.SourceWorkspaceArtifactId, jobHistoryError).ConfigureAwait(false);
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
			return specialField.DestinationFieldName;
		}

		private async Task<IEnumerable<string>> TagSourceDocumentsAsync(ISynchronizationConfiguration configuration, IEnumerable<string> documentIdentifiers, CancellationToken token)
		{
			var failedIdentifiers = new List<string>();
			IList<string> identifiersList = documentIdentifiers.ToList();
			if (identifiersList.Count > 0)
			{
				IList<TagDocumentsResult<string>> taggingResults = await _sourceWorkspaceTagRepository.TagDocumentsAsync(configuration, identifiersList, token).ConfigureAwait(false);
				foreach (TagDocumentsResult<string> taggingResult in taggingResults)
				{
					if (taggingResult.FailedDocuments.Any())
					{
						failedIdentifiers.AddRange(taggingResult.FailedDocuments);
					}
				}
			}
			return failedIdentifiers;
		}

		private async Task<IEnumerable<int>> TagDestinationDocumentsAsync(ISynchronizationConfiguration configuration, IEnumerable<int> artifactIds, CancellationToken token)
		{
			var failedArtifactIds = new List<int>();
			IList<int> artifactIdsList = artifactIds.ToList();
			if (artifactIdsList.Count > 0)
			{
				IList<TagDocumentsResult<int>> taggingResults = await _destinationWorkspaceTagRepository.TagDocumentsAsync(configuration, artifactIdsList, token).ConfigureAwait(false);
				foreach (TagDocumentsResult<int> taggingResult in taggingResults)
				{
					if (taggingResult.FailedDocuments.Any())
					{
						failedArtifactIds.AddRange(taggingResult.FailedDocuments);
					}
				}
			}
			return failedArtifactIds;
		}

		private async Task<ExecutionResult> GetTaggingResults<T>(IList<Task<IEnumerable<T>>> taggingTasks, int jobHistoryArtifactId)
		{
			ExecutionResult taggingResult = ExecutionResult.Success();
			var failedTagArtifactIds = new List<T>();
			try
			{
				await Task.WhenAll(taggingTasks).ConfigureAwait(false);
				foreach (Task<IEnumerable<T>> task in taggingTasks)
				{
					failedTagArtifactIds.AddRange(task.Result);
				}

				if (failedTagArtifactIds.Any())
				{
					const int maxSubset = 50;
					int subsetCount = failedTagArtifactIds.Count < maxSubset ? failedTagArtifactIds.Count : maxSubset;
					string subsetArtifactIds = string.Join(",", failedTagArtifactIds.Take(subsetCount));

					string errorMessage = $"Failed to tag synchronized documents in workspace. The first {maxSubset} out of {failedTagArtifactIds.Count} are: {subsetArtifactIds}.";
					var failedTaggingException = new SyncException(errorMessage, jobHistoryArtifactId.ToString(CultureInfo.InvariantCulture));
					taggingResult = ExecutionResult.Failure(errorMessage, failedTaggingException);
				}
			}
			catch (OperationCanceledException oce)
			{
				const string taggingCanceledMessage = "Tagging synchronized documents in workspace was interrupted due to the job being canceled.";
				_logger.LogInformation(oce, taggingCanceledMessage);
				taggingResult = new ExecutionResult(ExecutionStatus.Canceled, taggingCanceledMessage, oce);
			}
			catch (Exception ex)
			{
				const string message = "Unexpected exception occurred while tagging synchronized documents in workspace.";
				_logger.LogError(ex, message);
				taggingResult = ExecutionResult.Failure(message, ex);
			}
			return taggingResult;
		}
	}
}