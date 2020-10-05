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
		private readonly SyncJobParameters _syncJobParameters;
		private readonly ISyncLog _logger;

		public ImportJobFactory(IImportApiFactory importApiFactory, ISourceWorkspaceDataReaderFactory dataReaderFactory,
			IJobHistoryErrorRepository jobHistoryErrorRepository, IInstanceSettings instanceSettings, SyncJobParameters syncJobParameters, ISyncLog logger)
		{
			_importApiFactory = importApiFactory;
			_dataReaderFactory = dataReaderFactory;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
			_instanceSettings = instanceSettings;
			_syncJobParameters = syncJobParameters;
			_logger = logger;
		}

		public async Task<IImportJob> CreateImageImportJobAsync(ISynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
		{
			ISourceWorkspaceDataReader sourceWorkspaceDataReader = _dataReaderFactory.CreateImageSourceWorkspaceDataReader(batch, token);
			IImportAPI importApi = await GetImportApiAsync().ConfigureAwait(false);
			ImageImportBulkArtifactJob importJob = importApi.NewImageImportJob();

			//SetCommonIapiSettings(configuration, importJob.Settings, importApi);

			importJob.SourceData.Reader = sourceWorkspaceDataReader;

			importJob.Settings.ApplicationName = _syncJobParameters.SyncApplicationName;
			importJob.Settings.ArtifactTypeId = configuration.RdoArtifactTypeId;
			importJob.Settings.AuditLevel = kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel.FullAudit;
			importJob.Settings.AutoNumberImages = true;
			importJob.Settings.BatesNumberField = configuration.FileNameColumn;
			importJob.Settings.Billable = configuration.ImportImageFileCopyMode == ImportImageFileCopyMode.CopyFiles;
			importJob.Settings.CaseArtifactId = configuration.DestinationWorkspaceArtifactId;
			importJob.Settings.CopyFilesToDocumentRepository = configuration.ImportImageFileCopyMode == ImportImageFileCopyMode.CopyFiles;
			importJob.Settings.DestinationFolderArtifactID = configuration.DestinationFolderArtifactId;
			importJob.Settings.DisableImageTypeValidation = true;
			importJob.Settings.DocumentIdentifierField = GetSelectedIdentifierFieldName(
				importApi, configuration.DestinationWorkspaceArtifactId, configuration.RdoArtifactTypeId,
				configuration.IdentityFieldId);

			importJob.Settings.FileLocationField = configuration.ImageFilePathSourceFieldName;
			//importJob.Settings.FileNameField = ""; //TODO
			importJob.Settings.IdentityFieldId = configuration.IdentityFieldId;
			//importJob.Settings.FolderPathSourceFieldName
			importJob.Settings.MaximumErrorCount = int.MaxValue - 1; // From IAPI docs: This must be greater than 0 and less than Int32.MaxValue.
			importJob.Settings.NativeFileCopyMode = (NativeFileCopyModeEnum)configuration.ImportImageFileCopyMode;
			importJob.Settings.OverlayBehavior = (OverlayBehavior)configuration.FieldOverlayBehavior;
			importJob.Settings.OverwriteMode = (OverwriteModeEnum)configuration.ImportOverwriteMode;
			importJob.Settings.StartRecordNumber = 0;
			
			
			
			//importJob.Settings.StartRecordNumber = 0;
			//importJob.Settings.AuditLevel = kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel.FullAudit;
			//importJob.Settings.CaseArtifactId = configuration.DestinationWorkspaceArtifactId;
			//importJob.Settings.DestinationFolderArtifactID = configuration.DestinationFolderArtifactId;
			//importJob.Settings.MoveDocumentsInAppendOverlayMode =
			//	configuration.ImportOverwriteMode != ImportOverwriteMode.AppendOnly &&
			//	configuration.MoveExistingDocuments && !string.IsNullOrEmpty(configuration.FolderPathSourceFieldName);

			//importJob.Settings.OverlayBehavior = (OverlayBehavior)configuration.FieldOverlayBehavior;
			//importJob.Settings.OverwriteMode = (OverwriteModeEnum)configuration.ImportOverwriteMode;
			//importJob.Settings.IdentityFieldId = configuration.IdentityFieldId;

			//importJob.Settings.SelectedIdentifierFieldName = GetSelectedIdentifierFieldName(
			//	importApi, configuration.DestinationWorkspaceArtifactId, configuration.RdoArtifactTypeId,
			//	configuration.IdentityFieldId);

			//importJob.SourceData.Reader = sourceWorkspaceDataReader;
			//importJob.Settings.ArtifactTypeId = configuration.RdoArtifactTypeId;
			//importJob.Settings.FolderPathSourceFieldName = configuration.FolderPathSourceFieldName;
			//importJob.Settings.Billable = configuration.ImportImageFileCopyMode == ImportImageFileCopyMode.CopyFiles;
			//importJob.Settings.NativeFileCopyMode = (NativeFileCopyModeEnum)configuration.ImportImageFileCopyMode;
			//importJob.Settings.ImageFilePathSourceFieldName = configuration.ImageFilePathSourceFieldName;

			var syncImportBulkArtifactJob = new SyncImportBulkArtifactJob(importJob, sourceWorkspaceDataReader);

			ImportJob job = new ImportJob(syncImportBulkArtifactJob, new SemaphoreSlimWrapper(new SemaphoreSlim(0, 1)), _jobHistoryErrorRepository,
				configuration.SourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, _logger);

			return job;
		}

		public async Task<IImportJob> CreateNativeImportJobAsync(ISynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
		{
			ISourceWorkspaceDataReader sourceWorkspaceDataReader = _dataReaderFactory.CreateNativeSourceWorkspaceDataReader(batch, token);
			IImportAPI importApi = await GetImportApiAsync().ConfigureAwait(false);
			ImportBulkArtifactJob importJob = importApi.NewNativeDocumentImportJob();

			SetCommonIapiSettings(configuration, importJob.Settings, importApi);

			importJob.SourceData.SourceData = sourceWorkspaceDataReader; // This assignment invokes IDataReader.Read immediately!
			importJob.Settings.ArtifactTypeId = configuration.RdoArtifactTypeId;
			importJob.Settings.FolderPathSourceFieldName = configuration.FolderPathSourceFieldName;
			importJob.Settings.Billable = configuration.ImportNativeFileCopyMode == ImportNativeFileCopyMode.CopyFiles;
			importJob.Settings.NativeFileCopyMode = (NativeFileCopyModeEnum)configuration.ImportNativeFileCopyMode;
			importJob.Settings.DisableNativeLocationValidation = configuration.ImportNativeFileCopyMode == ImportNativeFileCopyMode.SetFileLinks;

			importJob.Settings.MultiValueDelimiter = configuration.MultiValueDelimiter;
			importJob.Settings.NestedValueDelimiter = configuration.NestedValueDelimiter;

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

			var syncImportBulkArtifactJob = new SyncImportBulkArtifactJob(importJob, sourceWorkspaceDataReader);

			ImportJob job = new ImportJob(syncImportBulkArtifactJob, new SemaphoreSlimWrapper(new SemaphoreSlim(0, 1)), _jobHistoryErrorRepository,
				configuration.SourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, _logger);

			return job;
		}

		private void SetCommonIapiSettings(ISynchronizationConfiguration configuration, ImportSettingsBase settings, IImportAPI importApi)
		{
			settings.ApplicationName = _syncJobParameters.SyncApplicationName;
			settings.MaximumErrorCount = int.MaxValue - 1; // From IAPI docs: This must be greater than 0 and less than Int32.MaxValue.
			settings.StartRecordNumber = 0;
			settings.AuditLevel = kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel.FullAudit;
			settings.CaseArtifactId = configuration.DestinationWorkspaceArtifactId;
			settings.DestinationFolderArtifactID = configuration.DestinationFolderArtifactId;
			settings.MoveDocumentsInAppendOverlayMode =
				configuration.ImportOverwriteMode != ImportOverwriteMode.AppendOnly &&
				configuration.MoveExistingDocuments && !string.IsNullOrEmpty(configuration.FolderPathSourceFieldName);

			settings.OverlayBehavior = (OverlayBehavior) configuration.FieldOverlayBehavior;
			settings.OverwriteMode = (OverwriteModeEnum) configuration.ImportOverwriteMode;
			settings.IdentityFieldId = configuration.IdentityFieldId;

			settings.SelectedIdentifierFieldName = GetSelectedIdentifierFieldName(
				importApi, configuration.DestinationWorkspaceArtifactId, configuration.RdoArtifactTypeId,
				configuration.IdentityFieldId);
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

		private static string GetSelectedIdentifierFieldName(IImportAPI importApi, int workspaceArtifactId, int artifactTypeId, int identityFieldArtifactId)
		{
			IEnumerable<Field> workspaceFields = importApi.GetWorkspaceFields(workspaceArtifactId, artifactTypeId);
			Field identityField = workspaceFields.First(x => x.ArtifactID == identityFieldArtifactId);
			return identityField.Name;
		}
	}
}