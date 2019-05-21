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
		private readonly IImportJobFactory _importJobFactory;
		private readonly IBatchRepository _batchRepository;
		private readonly ISyncMetrics _syncMetrics;
		private readonly IDateTime _dateTime;
		private readonly IFieldManager _fieldManager;
		private readonly ISyncLog _logger;

		public SynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository, ISyncMetrics syncMetrics, IDateTime dateTime, IFieldManager fieldManager, ISyncLog logger)
		{
			_importJobFactory = importJobFactory;
			_batchRepository = batchRepository;
			_syncMetrics = syncMetrics;
			_dateTime = dateTime;
			_fieldManager = fieldManager;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ISynchronizationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Creating settings for ImportAPI.");
			ImportSettingsDto importSettings = GetImportSettings();
			configuration.SetImportSettings(importSettings);

			ExecutionResult result = ExecutionResult.Success();
			DateTime startTime = _dateTime.Now;

			try
			{
				_logger.LogVerbose("Gathering batches to execute.");
				List<int> batchesIds = (await _batchRepository.GetAllNewBatchesIdsAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false)).ToList();

				foreach (int batchId in batchesIds)
				{
					if (token.IsCancellationRequested)
					{
						_logger.LogInformation("Import job has been canceled.");
						result = ExecutionResult.Canceled();
						break;
					}

					IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId).ConfigureAwait(false);
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

			return result;
		}
		
		private ImportSettingsDto GetImportSettings()
		{
			List<FieldInfoDto> specialFields = _fieldManager.GetSpecialFields().ToList();

			ImportSettingsDto importSettings = new ImportSettingsDto
			{
				FolderPathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.FolderPath),
				FileSizeColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileSize),
				NativeFilePathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileLocation),
				FileNameColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.NativeFileFilename),
				OiFileTypeColumnName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.RelativityNativeType),
				SupportedByViewerColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.SupportedByViewer)
			};
			return importSettings;
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
	}
}