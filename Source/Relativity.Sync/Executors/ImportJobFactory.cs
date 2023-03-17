using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.EDDS.WebAPI.BulkImportManagerBase;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Relativity.AntiMalware.SDK;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Executors
{
    internal sealed class ImportJobFactory : IImportJobFactory
    {
        private readonly IImportApiFactory _importApiFactory;
        private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
        private readonly ISourceWorkspaceDataReaderFactory _dataReaderFactory;
        private readonly SyncJobParameters _syncJobParameters;
        private readonly IFieldMappings _fieldMappings;
        private readonly IAntiMalwareEventHelper _antiMalwareEventHelper;
        private readonly ISyncToggles _syncToggles;
        private readonly IAPILog _logger;

        public ImportJobFactory(
            IImportApiFactory importApiFactory,
            ISourceWorkspaceDataReaderFactory dataReaderFactory,
            IJobHistoryErrorRepository jobHistoryErrorRepository,
            SyncJobParameters syncJobParameters,
            IFieldMappings fieldMappings,
            IAntiMalwareEventHelper antiMalwareEventHelper,
            ISyncToggles syncToggles,
            IAPILog logger)
        {
            _importApiFactory = importApiFactory;
            _dataReaderFactory = dataReaderFactory;
            _jobHistoryErrorRepository = jobHistoryErrorRepository;
            _syncJobParameters = syncJobParameters;
            _fieldMappings = fieldMappings;
            _antiMalwareEventHelper = antiMalwareEventHelper;
            _syncToggles = syncToggles;
            _logger = logger;
        }

        public async Task<IImportJob> CreateRdoImportJobAsync(INonDocumentSynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
        {
            ISourceWorkspaceDataReader sourceWorkspaceDataReader = _dataReaderFactory.CreateNonDocumentSourceWorkspaceDataReader(batch, token);
            IImportAPI importApi = await _importApiFactory.CreateImportApiAsync().ConfigureAwait(false);
            ImportBulkArtifactJob importJob = importApi.NewObjectImportJob(configuration.DestinationRdoArtifactTypeId);

            SetCommonIapiSettings(configuration, importJob.Settings);

            importJob.SourceData.SourceData = sourceWorkspaceDataReader;
            importJob.Settings.ArtifactTypeId = configuration.DestinationRdoArtifactTypeId;

            importJob.Settings.MultiValueDelimiter = LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII;
            importJob.Settings.NestedValueDelimiter = LoadFileOptions._DEFAULT_NESTED_VALUE_ASCII;

            importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.DoNotImportNativeFiles;
            importJob.Settings.SelectedIdentifierFieldName = GetIdentifierFieldName();

            var syncImportBulkArtifactJob = new SyncImportBulkArtifactJob(importJob, sourceWorkspaceDataReader, _antiMalwareEventHelper, configuration.SourceWorkspaceArtifactId, _logger);

            ImportJob job = new ImportJob(syncImportBulkArtifactJob, new SemaphoreSlimWrapper(new SemaphoreSlim(0, 1)), _jobHistoryErrorRepository, configuration.SourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, _logger);

            _logger.LogInformation("Import Settings: {@settings}", NonDocumentImportSettingsForLogging.CreateWithoutSensitiveData(importJob.Settings));

            return job;
        }

        public async Task<IImportJob> CreateRdoLinkingJobAsync(INonDocumentSynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
        {
            ISourceWorkspaceDataReader sourceWorkspaceDataReader = _dataReaderFactory.CreateNonDocumentObjectLinkingSourceWorkspaceDataReader(batch, token);
            IImportAPI importApi = await _importApiFactory.CreateImportApiAsync().ConfigureAwait(false);
            ImportBulkArtifactJob importJob = importApi.NewObjectImportJob(configuration.DestinationRdoArtifactTypeId);

            SetCommonIapiSettings(configuration, importJob.Settings);

            importJob.Settings.OverwriteMode = OverwriteModeEnum.Overlay;

            importJob.SourceData.SourceData = sourceWorkspaceDataReader;
            importJob.Settings.ArtifactTypeId = configuration.DestinationRdoArtifactTypeId;

            importJob.Settings.MultiValueDelimiter = LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII;
            importJob.Settings.NestedValueDelimiter = LoadFileOptions._DEFAULT_NESTED_VALUE_ASCII;

            importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.DoNotImportNativeFiles;
            importJob.Settings.SelectedIdentifierFieldName = GetIdentifierFieldName();

            var syncImportBulkArtifactJob = new SyncImportBulkArtifactJob(importJob, sourceWorkspaceDataReader, _antiMalwareEventHelper, configuration.SourceWorkspaceArtifactId, _logger);

            ImportJob job = new ImportJob(syncImportBulkArtifactJob, new SemaphoreSlimWrapper(new SemaphoreSlim(0, 1)), _jobHistoryErrorRepository, configuration.SourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, _logger);

            _logger.LogInformation("Import Settings: {@settings}", NonDocumentImportSettingsForLogging.CreateWithoutSensitiveData(importJob.Settings));

            return job;
        }

        public async Task<IImportJob> CreateImageImportJobAsync(IImageSynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
        {
            ISourceWorkspaceDataReader sourceWorkspaceDataReader = _dataReaderFactory.CreateImageSourceWorkspaceDataReader(batch, token);
            IImportAPI importApi = await _importApiFactory.CreateImportApiAsync().ConfigureAwait(false);
            ImageImportBulkArtifactJob importJob = importApi.NewImageImportJob();

            SetCommonIapiSettings(configuration, importJob.Settings);

            importJob.SourceData.Reader = sourceWorkspaceDataReader;
            importJob.Settings.ArtifactTypeId = configuration.RdoArtifactTypeId;
            importJob.Settings.AutoNumberImages = true;
            importJob.Settings.BatesNumberField = configuration.IdentifierColumn;
            importJob.Settings.Billable = configuration.ImportImageFileCopyMode == ImportImageFileCopyMode.CopyFiles;
            importJob.Settings.CopyFilesToDocumentRepository = configuration.ImportImageFileCopyMode == ImportImageFileCopyMode.CopyFiles;
            importJob.Settings.DisableImageTypeValidation = true;
            importJob.Settings.DocumentIdentifierField = GetSelectedIdentifierFieldName(
                importApi,
                configuration.DestinationWorkspaceArtifactId,
                configuration.RdoArtifactTypeId,
                configuration.IdentityFieldId);

            importJob.Settings.FileNameField = configuration.FileNameColumn;
            importJob.Settings.FileLocationField = configuration.ImageFilePathSourceFieldName;
            importJob.Settings.NativeFileCopyMode = (NativeFileCopyModeEnum)configuration.ImportImageFileCopyMode;

            var syncImportBulkArtifactJob = new SyncImportBulkArtifactJob(importJob, sourceWorkspaceDataReader, _antiMalwareEventHelper, configuration.SourceWorkspaceArtifactId, _logger);

            ImportJob job = new ImportJob(
                syncImportBulkArtifactJob,
                new SemaphoreSlimWrapper(new SemaphoreSlim(0, 1)),
                _jobHistoryErrorRepository,
                configuration.SourceWorkspaceArtifactId,
                configuration.JobHistoryArtifactId,
                _logger);

            _logger.LogInformation(
                "Import Settings: {@settings}",
                ImageImportSettingsForLogging.CreateWithoutSensitiveData(importJob.Settings));

            return job;
        }

        public async Task<IImportJob> CreateNativeImportJobAsync(IDocumentSynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
        {
            ISourceWorkspaceDataReader sourceWorkspaceDataReader = _dataReaderFactory.CreateNativeSourceWorkspaceDataReader(batch, token);
            IImportAPI importApi = await _importApiFactory.CreateImportApiAsync().ConfigureAwait(false);
            ImportBulkArtifactJob importJob = importApi.NewNativeDocumentImportJob();

            SetCommonIapiSettings(configuration, importJob.Settings);

            importJob.SourceData.SourceData = sourceWorkspaceDataReader; // This assignment invokes IDataReader.Read immediately!
            importJob.Settings.ArtifactTypeId = configuration.RdoArtifactTypeId;
            importJob.Settings.FolderPathSourceFieldName = configuration.FolderPathSourceFieldName;
            importJob.Settings.Billable = configuration.ImportNativeFileCopyMode == ImportNativeFileCopyMode.CopyFiles;

            importJob.Settings.MultiValueDelimiter = LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII;
            importJob.Settings.NestedValueDelimiter = LoadFileOptions._DEFAULT_NESTED_VALUE_ASCII;

            importJob.Settings.NativeFileCopyMode = (NativeFileCopyModeEnum)configuration.ImportNativeFileCopyMode;
            importJob.Settings.DisableNativeLocationValidation = configuration.ImportNativeFileCopyMode == ImportNativeFileCopyMode.SetFileLinks;

            if (configuration.ImportNativeFileCopyMode != ImportNativeFileCopyMode.DoNotImportNativeFiles)
            {
                importJob.Settings.NativeFilePathSourceFieldName = configuration.NativeFilePathSourceFieldName;
                importJob.Settings.OIFileIdMapped = true;
                importJob.Settings.FileSizeMapped = true;
                importJob.Settings.FileSizeColumn = configuration.FileSizeColumn;
                importJob.Settings.FileNameColumn = configuration.FileNameColumn;
                importJob.Settings.OIFileTypeColumnName = configuration.OiFileTypeColumnName;
                importJob.Settings.SupportedByViewerColumn = configuration.SupportedByViewerColumn;

                // Do not set DisableNativeValidation to "true" - Import API will ignore "Relativity Native Type" field
                // and will set it to "Unknown format". Overwriting "OIFileTypeColumnName" and "SupportedByViewerColumn"
                // in the settings is enough to disable calling OutsideIn - instead it will take the values from supplied IDataReader.
                importJob.Settings.DisableNativeValidation = false;
            }

            importJob.Settings.SelectedIdentifierFieldName = GetSelectedIdentifierFieldName(
                importApi,
                configuration.DestinationWorkspaceArtifactId,
                configuration.RdoArtifactTypeId,
                configuration.IdentityFieldId);

            var syncImportBulkArtifactJob = new SyncImportBulkArtifactJob(importJob, sourceWorkspaceDataReader, _antiMalwareEventHelper, configuration.SourceWorkspaceArtifactId, _logger);

            ImportJob job = new ImportJob(
                syncImportBulkArtifactJob,
                new SemaphoreSlimWrapper(new SemaphoreSlim(0, 1)),
                _jobHistoryErrorRepository,
                configuration.SourceWorkspaceArtifactId,
                configuration.JobHistoryArtifactId,
                _logger);

            _logger.LogInformation(
                "Import Settings: {@settings}",
                NativeImportSettingsForLogging.CreateWithoutSensitiveData(importJob.Settings));

            return job;
        }

        private void SetCommonIapiSettings(ISynchronizationConfiguration configuration, ImportSettingsBase settings)
        {
            settings.ApplicationName = _syncJobParameters.SyncApplicationName;
            settings.MaximumErrorCount = int.MaxValue - 1; // From IAPI docs: This must be greater than 0 and less than Int32.MaxValue.
            settings.StartRecordNumber = 0;
            settings.CaseArtifactId = configuration.DestinationWorkspaceArtifactId;
            settings.DestinationFolderArtifactID = configuration.DestinationFolderArtifactId;
            settings.MoveDocumentsInAppendOverlayMode =
                configuration.ImportOverwriteMode != ImportOverwriteMode.AppendOnly &&
                configuration.MoveExistingDocuments && !string.IsNullOrEmpty(configuration.FolderPathSourceFieldName);

            settings.OverlayBehavior = (OverlayBehavior)configuration.FieldOverlayBehavior;
            settings.OverwriteMode = (OverwriteModeEnum)configuration.ImportOverwriteMode;

            settings.IdentityFieldId = configuration.IdentityFieldId;

            if (_syncToggles.IsEnabled<EnableAuditToggle>())
            {
                _logger.LogInformation("Running IAPI with FullAudit mode.");
                settings.AuditLevel = kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel.FullAudit;
            }
            else
            {
                _logger.LogInformation("Running IAPI with NoAudit.");
                settings.AuditLevel = kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel.NoAudit;
            }
        }

        private static string GetSelectedIdentifierFieldName(IImportAPI importApi, int workspaceArtifactId, int artifactTypeId, int identityFieldArtifactId)
        {
            IEnumerable<Field> workspaceFields = importApi.GetWorkspaceFields(workspaceArtifactId, artifactTypeId);
            Field identityField = workspaceFields.First(x => x.ArtifactID == identityFieldArtifactId);
            return identityField.Name;
        }

        private string GetIdentifierFieldName()
        {
            return _fieldMappings.GetFieldMappings().First(x => x.FieldMapType == FieldMapType.Identifier).DestinationField.DisplayName;
        }
    }
}
