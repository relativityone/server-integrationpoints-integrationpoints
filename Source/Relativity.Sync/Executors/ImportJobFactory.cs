using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.EDDS.WebAPI.BulkImportManagerBase;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	internal sealed class ImportJobFactory : IImportJobFactory
	{
		private readonly IImportApiFactory _importApiFactory;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly IInstanceSettings _instanceSettings;
		private readonly ISourceWorkspaceDataReaderFactory _dataReaderFactory;
		private readonly ISyncLog _logger;

		public ImportJobFactory(IImportApiFactory importApiFactory, ISourceWorkspaceDataReaderFactory dataReaderFactory,
			IJobHistoryErrorRepository jobHistoryErrorRepository,
			IInstanceSettings instanceSettings, ISyncLog logger)
		{
			_importApiFactory = importApiFactory;
			_dataReaderFactory = dataReaderFactory;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
			_instanceSettings = instanceSettings;
			_logger = logger;
		}

		public async Task<IImportJob> CreateImportJobAsync(ISynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
		{
			ISourceWorkspaceDataReader sourceWorkspaceDataReader = _dataReaderFactory.CreateSourceWorkspaceDataReader(batch, token);
			ImportBulkArtifactJob importBulkArtifactJob = await CreateImportBulkArtifactJobAsync(configuration, sourceWorkspaceDataReader).ConfigureAwait(false);
			var syncImportBulkArtifactJob = new SyncImportBulkArtifactJob(importBulkArtifactJob, sourceWorkspaceDataReader.ItemStatusMonitor);

			return new ImportJob(syncImportBulkArtifactJob, new SemaphoreSlimWrapper(new SemaphoreSlim(0, 1)), _jobHistoryErrorRepository,
				configuration.SourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, _logger);
		}

	
		private async Task<ImportBulkArtifactJob> CreateImportBulkArtifactJobAsync(ISynchronizationConfiguration configuration, ISourceWorkspaceDataReader dataReader, int startingIndex = 0)
		{
			IImportAPI importApi = await GetImportApiAsync().ConfigureAwait(false);
			ImportBulkArtifactJob importJob = await Task.Run(() => importApi.NewNativeDocumentImportJob()).ConfigureAwait(false);

			importJob.SourceData.SourceData = dataReader;
			importJob.Settings.MaximumErrorCount = int.MaxValue - 1; // From IAPI docs: This must be greater than 0 and less than Int32.MaxValue.
			importJob.Settings.StartRecordNumber = startingIndex;
			importJob.Settings.ArtifactTypeId = configuration.RdoArtifactTypeId;
			importJob.Settings.AuditLevel = kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel.FullAudit;
			importJob.Settings.MultiValueDelimiter = configuration.MultiValueDelimiter;
			importJob.Settings.NestedValueDelimiter = configuration.NestedValueDelimiter;
			importJob.Settings.CaseArtifactId = configuration.DestinationWorkspaceArtifactId;
			importJob.Settings.DestinationFolderArtifactID = configuration.DestinationFolderArtifactId;
			importJob.Settings.MoveDocumentsInAppendOverlayMode = configuration.ImportOverwriteMode != ImportOverwriteMode.AppendOnly &&
				configuration.MoveExistingDocuments && !string.IsNullOrEmpty(configuration.FolderPathSourceFieldName);
			importJob.Settings.Billable = configuration.ImportNativeFileCopyMode == ImportNativeFileCopyMode.CopyFiles;

			importJob.Settings.NativeFileCopyMode = (NativeFileCopyModeEnum)configuration.ImportNativeFileCopyMode;
			importJob.Settings.OverlayBehavior = (OverlayBehavior)configuration.FieldOverlayBehavior;
			importJob.Settings.OverwriteMode = (OverwriteModeEnum)configuration.ImportOverwriteMode;
			importJob.Settings.IdentityFieldId = configuration.IdentityFieldId;
			importJob.Settings.FolderPathSourceFieldName = configuration.FolderPathSourceFieldName;
			importJob.Settings.FileSizeColumn = configuration.FileSizeColumn;
			importJob.Settings.FileNameColumn = configuration.FileNameColumn;
			importJob.Settings.OIFileTypeColumnName = configuration.OiFileTypeColumnName;
			importJob.Settings.SupportedByViewerColumn = configuration.SupportedByViewerColumn;

			if (configuration.ImportNativeFileCopyMode != ImportNativeFileCopyMode.DoNotImportNativeFiles)
			{
				importJob.Settings.NativeFilePathSourceFieldName = configuration.NativeFilePathSourceFieldName;
				importJob.Settings.OIFileIdMapped = true;
				importJob.Settings.FileSizeMapped = true;
				importJob.Settings.DisableNativeValidation = false;
			}

			importJob.Settings.DisableNativeLocationValidation = configuration.ImportNativeFileCopyMode == ImportNativeFileCopyMode.SetFileLinks;

			importJob.Settings.SelectedIdentifierFieldName = await GetSelectedIdentifierFieldNameAsync(
				importApi, configuration.DestinationWorkspaceArtifactId, configuration.RdoArtifactTypeId, configuration.IdentityFieldId).ConfigureAwait(false);

			return importJob;
		}

		private async Task<IImportAPI> GetImportApiAsync()
		{
			string webApiPath = await _instanceSettings.GetWebApiPathAsync().ConfigureAwait(false);
			if(Uri.IsWellFormedUriString(webApiPath, UriKind.Absolute))
			{
				var webApiUri = new Uri(webApiPath);
				return await _importApiFactory.CreateImportApiAsync(webApiUri).ConfigureAwait(false);
			}
			else
			{
				string invalidWebAPIPathMessage = string.IsNullOrEmpty(webApiPath)
					? "WebAPIPath doesn't exist"
					: $"WebAPIPath {webApiPath} is invalid";
				_logger.LogError(invalidWebAPIPathMessage);
				throw new ImportFailedException(invalidWebAPIPathMessage);
			}
		}

		private static async Task<string> GetSelectedIdentifierFieldNameAsync(IImportAPI importApi, int workspaceArtifactId, int artifactTypeId, int identityFieldArtifactId)
		{
			IEnumerable<Field> workspaceFields = await Task.Run(() => importApi.GetWorkspaceFields(workspaceArtifactId, artifactTypeId)).ConfigureAwait(false);
			Field identityField = workspaceFields.First(x => x.ArtifactID == identityFieldArtifactId);
			return identityField.Name;
		}
	}
}