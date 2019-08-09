using System;
using System.Collections.Generic;
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
		private readonly IImportJobFactory _importJobFactory;
		private readonly IFieldManager _fieldManager;
		private readonly IFieldMappings _fieldMappings;
		private readonly ISyncLog _logger;
		private readonly IDocumentsTagRepository _documentsTagRepository;

		public SynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository, IFieldManager fieldManager, 
			IFieldMappings fieldMappings, ISyncLog logger, IDocumentsTagRepository documentsTagRepository)
		{
			_batchRepository = batchRepository;
			_importJobFactory = importJobFactory;
			_fieldManager = fieldManager;
			_fieldMappings = fieldMappings;
			_logger = logger;
			_documentsTagRepository = documentsTagRepository;
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
					using (IImportJob importJob = await _importJobFactory.CreateImportJobAsync(configuration, batch, token).ConfigureAwait(false))
					{
						importResult = await importJob.RunAsync(token).ConfigureAwait(false);

						IEnumerable<int> pushedDocumentArtifactIds = await importJob.GetPushedDocumentArtifactIds().ConfigureAwait(false);
						Task<IEnumerable<int>> destinationTaggingTask = _documentsTagRepository.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(configuration, pushedDocumentArtifactIds, token);
						destinationTaggingTasks.Add(destinationTaggingTask);

						IEnumerable<string> pushedDocumentIdentifiers = await importJob.GetPushedDocumentIdentifiers().ConfigureAwait(false);
						Task<IEnumerable<string>> sourceTaggingTask = _documentsTagRepository.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(configuration, pushedDocumentIdentifiers, token);
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

			ExecutionResult destinationTaggingResult = await _documentsTagRepository.GetTaggingResults(destinationTaggingTasks, configuration.JobHistoryArtifactId).ConfigureAwait(false);
			if (destinationTaggingResult.Status == ExecutionStatus.Failed)
			{
				await _documentsTagRepository.GenerateDocumentTaggingJobHistoryError(destinationTaggingResult, configuration).ConfigureAwait(false);
			}
			ExecutionResult sourceTaggingResult = await _documentsTagRepository.GetTaggingResults(sourceTaggingTasks, configuration.JobHistoryArtifactId).ConfigureAwait(false);
			if (sourceTaggingResult.Status == ExecutionStatus.Failed)
			{
				await _documentsTagRepository.GenerateDocumentTaggingJobHistoryError(sourceTaggingResult, configuration).ConfigureAwait(false);
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
			var jobHistoryError = new CreateJobHistoryErrorDto(ErrorType.Job)
			{
				ErrorMessage = taggingResult.Message,
				StackTrace = taggingResult.Exception?.StackTrace
			};
			await _jobHistoryErrorRepository.CreateAsync(configuration.SourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, jobHistoryError)
				.ConfigureAwait(false);
		}

		private void UpdateImportSettings(ISynchronizationConfiguration configuration)
		{
			int destinationIdentityFieldId = GetDestinationIdentityFieldId(_fieldMappings.GetFieldMappings());
			IList<FieldInfoDto> specialFields = _fieldManager.GetSpecialFields().ToList();

			configuration.ImportSettings.IdentityFieldId = destinationIdentityFieldId;
			if (configuration.DestinationFolderStructureBehavior != DestinationFolderStructureBehavior.None)
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
	}
}