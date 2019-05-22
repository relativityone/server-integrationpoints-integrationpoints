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
		private readonly IDateTime _dateTime;
		private readonly IDestinationWorkspaceTagRepository _destinationWorkspaceTagRepository;
		private readonly IImportJobFactory _importJobFactory;
		private readonly IFieldManager _fieldManager;
		private readonly ISyncLog _logger;
		private readonly ISyncMetrics _syncMetrics;

		public SynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository,
			IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository, ISyncMetrics syncMetrics, IDateTime dateTime, IFieldManager fieldManager, ISyncLog logger)
		{
			_batchRepository = batchRepository;
			_dateTime = dateTime;
			_destinationWorkspaceTagRepository = destinationWorkspaceTagRepository;
			_importJobFactory = importJobFactory;
			_syncMetrics = syncMetrics;
			_dateTime = dateTime;
			_fieldManager = fieldManager;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ISynchronizationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Creating settings for ImportAPI.");
			ImportSettingsDto importSettings = GetImportSettingsAsync(configuration);
			configuration.SetImportSettings(importSettings);
			
			ExecutionResult result = ExecutionResult.Success();
			DateTime startTime = _dateTime.Now;

			IList<List<int>> batchArtifactIds = new List<List<int>>();

			try
			{
				_logger.LogVerbose("Gathering batches to execute.");
				IList<int> batchIds = (await _batchRepository.GetAllNewBatchesIdsAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false)).ToList();

				foreach (int batchId in batchIds)
				{
					if (token.IsCancellationRequested)
					{
						_logger.LogInformation("Import job has been canceled.");
						result = ExecutionResult.Canceled();
						break;
					}

					IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId).ConfigureAwait(false);
					batchArtifactIds.Add((await batch.GetItemArtifactIds(configuration.ExportRunId).ConfigureAwait(false)).ToList());

					_logger.LogVerbose("Processing batch ID: {batchId}", batchId);
					using (IImportJob importJob = _importJobFactory.CreateImportJob(configuration, batch))
					{
						await importJob.RunAsync(token).ConfigureAwait(false);
					}
					_logger.LogInformation("Batch ID: {batchId} processed successfully.", batchId);
				}
			}
			catch (SyncException ex)
			{
				const string message = "Fatal exception occurred while executing import job.";
				_logger.LogError(ex, message);
				result = ExecutionResult.Failure(message, ex);
			}
			catch (Exception ex)
			{
				const string message = "Unexpected exception occurred while executing import job.";
				_logger.LogError(ex, message);
				result = ExecutionResult.Failure(message, ex);
			}
			finally
			{
				// TODO metrics
				DateTime endTime = _dateTime.Now;
				TimeSpan jobDuration = endTime - startTime;
				_syncMetrics.CountOperation("ImportJobStatus", result.Status);
				_syncMetrics.TimedOperation("ImportJob", jobDuration, result.Status);
				_syncMetrics.GaugeOperation("ImportJobStart", result.Status, startTime.Ticks, "Ticks", null);
				_syncMetrics.GaugeOperation("ImportJobEnd", result.Status, endTime.Ticks, "Ticks", null);
			}

			try
			{
				await TagDocumentsAsync(configuration, batchArtifactIds, token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				const string message = "Unexpected exception occurred while tagging synchronized documents in source workspace.";
				_logger.LogError(ex, message);

				if (result.Status == ExecutionStatus.Failed)
				{
					string aggregateMessage = result.Message + " " + message;
					var combinedException = new AggregateException(aggregateMessage, result.Exception, ex);
					result = ExecutionResult.Failure(aggregateMessage, combinedException);
				}
				else
				{
					result = ExecutionResult.Failure(message, ex);
				}
			}

			return result;
		}
		
		private ImportSettingsDto GetImportSettingsAsync(ISynchronizationConfiguration configuration)
		{
			int destinationIdentityFieldId = GetDestinationIdentityFieldId(configuration.FieldMappings);
			IList<FieldInfoDto> specialFields = _fieldManager.GetSpecialFields().ToList();

			ImportSettingsDto importSettings = new ImportSettingsDto
			{
				FolderPathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.FolderPath),
				FileSizeColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileSize),
				NativeFilePathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileLocation),
				FileNameColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileFilename),
				OiFileTypeColumnName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.RelativityNativeType),
				SupportedByViewerColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.SupportedByViewer),
				
				CaseArtifactId = configuration.DestinationWorkspaceArtifactId,
				IdentityFieldId = destinationIdentityFieldId,

				ImportOverwriteMode = configuration.ImportSettings.ImportOverwriteMode,
				FieldOverlayBehavior = configuration.ImportSettings.FieldOverlayBehavior,
				ImportNativeFileCopyMode = configuration.ImportSettings.ImportNativeFileCopyMode,
				CopyFilesToDocumentRepository = true
			};
			return importSettings;
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

		private async Task TagDocumentsAsync(ISynchronizationConfiguration configuration, IList<List<int>> artifactIds, CancellationToken token)
		{
			if (artifactIds.Any())
			{
				var tasks = new Task<IList<TagDocumentsResult>>[artifactIds.Count];

				for (int i = 0; i < artifactIds.Count; i++)
				{
					Task<IList<TagDocumentsResult>> tagTask = _destinationWorkspaceTagRepository.TagDocumentsAsync(configuration, artifactIds[i], token);
					tasks[i] = tagTask;
				}

				await Task.WhenAll(tasks).ConfigureAwait(false);
			}
		}
	}
}