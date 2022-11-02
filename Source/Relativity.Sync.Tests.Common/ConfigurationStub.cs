using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;
using IConfiguration = Relativity.Sync.Configuration.IConfiguration;

namespace Relativity.Sync.Tests.Common
{
    [ExcludeFromCodeCoverage]
    internal sealed class ConfigurationStub : IConfiguration, IDataDestinationFinalizationConfiguration, IDataDestinationInitializationConfiguration, IDataSourceSnapshotConfiguration, INonDocumentDataSourceSnapshotConfiguration,
        IDestinationWorkspaceObjectTypesCreationConfiguration, IDestinationWorkspaceSavedSearchCreationConfiguration, IDestinationWorkspaceTagsCreationConfiguration, IJobCleanupConfiguration,
        IJobStatusConsolidationConfiguration, INotificationConfiguration, IPermissionsCheckConfiguration, ISnapshotPartitionConfiguration, IObjectLinkingSnapshotPartitionConfiguration,
        ISourceWorkspaceTagsCreationConfiguration, ISynchronizationConfiguration, IValidationConfiguration, IUserContextConfiguration, IFieldConfiguration, IImageRetrieveConfiguration,
        IJobEndMetricsConfiguration, IAutomatedWorkflowTriggerConfiguration, IRetryDataSourceSnapshotConfiguration, IPipelineSelectorConfiguration,
        IDocumentSynchronizationConfiguration, INonDocumentSynchronizationConfiguration, IImageSynchronizationConfiguration, IPreValidationConfiguration, IRdoGuidConfiguration,
        IImageJobStartMetricsConfiguration, IDocumentJobStartMetricsConfiguration, ISnapshotQueryConfiguration, IMetricsConfiguration, IStatisticsConfiguration, INonDocumentJobStartMetricsConfiguration, IJobHistoryErrorRepositoryConfigration, INonDocumentObjectLinkingConfiguration,
        IAntiMalwareConfiguration, IBatchDataSourcePreparationConfiguration, IDocumentSynchronizationMonitorConfiguration, IConfigureDocumentSynchronizationConfiguration, IIAPIv2RunCheckerConfiguration
    {
        private IList<FieldMap> _fieldMappings = new List<FieldMap>();
        private string _jobName = String.Empty;
        private string _sourceJobTagName = String.Empty;
        private string _emailNotificationRecipients;
        private Guid? _jobStatusConsolidationConfigurationExportRunId;

        private const int _ADMIN_ID = 9;
        private const int _ASCII_GROUP_SEPARATOR = 29;
        private const int _ASCII_RECORD_SEPARATOR = 30;
        private const int _BATCH_SIZE_FOR_FILE_QUERIES = 10000;

        private readonly IEnumerable<string> _emailRecipients = new List<string>();

        public string DataDestinationName { get; set; }

        public bool IsDataDestinationArtifactIdSet { get; set; }

        public int DataDestinationArtifactId { get; set; }

        public int DataSourceArtifactId { get; set; }

        public string FolderPathField { get; set; }

        public IList<FieldMap> GetFieldMappings() => _fieldMappings;

        public void SetFieldMappings(IList<FieldMap> fieldMappings)
        {
            _fieldMappings = fieldMappings;
        }

        public bool IsSnapshotCreated { get; set; }

        public async Task SetSnapshotDataAsync(Guid runId, int totalRecordsCount)
        {
            await Task.Yield();
            ExportRunId = runId;
            TotalRecordsCount = totalRecordsCount;
            IsSnapshotCreated = true;
        }

        public string SourceWorkspaceTagName { get; set; }

        public bool CreateSavedSearchForTags { get; set; }

        public bool IsSavedSearchArtifactIdSet { get; set; }

        public async Task SetSavedSearchInDestinationArtifactIdAsync(int artifactId)
        {
            SavedSearchArtifactId = artifactId;
            IsSavedSearchArtifactIdSet = true;
            await Task.Yield();
        }

        public string GetJobName() => _jobName;

        public void SetJobName(string jobName)
        {
            _jobName = jobName;
        }

        public IEnumerable<string> GetEmailRecipients() => _emailRecipients;

        public string GetNotificationEmails() => _emailNotificationRecipients;

        public void SetEmailNotificationRecipients(string emailNotificationRecipients)
        {
            _emailNotificationRecipients = emailNotificationRecipients;
        }

        public DestinationLocationType DataDestinationType { get; }

        public int SourceWorkspaceArtifactId { get; set; }

        public string TriggerName { get; }

        public int SyncConfigurationArtifactId { get; set; }

        public ExecutionResult SynchronizationExecutionResult { get; set; }

        public string TriggerId { get; }

        public string TriggerValue { get; }

        public bool MoveExistingDocuments { get; set; }

        public int RdoArtifactTypeId { get; set; } = (int)ArtifactType.Document;

        public bool IsRetried { get; }

        public bool IsDrainStopped { get; }

        public bool HasLongTextFields { get; }

        public int DestinationRdoArtifactTypeId { get; set; } = (int)ArtifactType.Document;

        public string GetSourceWorkspaceTag() => string.Empty;

        public char MultiValueDelimiter => (char)_ASCII_RECORD_SEPARATOR;

        public char NestedValueDelimiter => (char)_ASCII_GROUP_SEPARATOR;

        public int DestinationWorkspaceArtifactId { get; set; }

        public int SavedSearchArtifactId { get; set; }

        public int DestinationFolderArtifactId { get; set; }

        public int SourceProviderArtifactId { get; }

        public string FolderPathSourceFieldName { get; set; }

        public string GetFolderPathSourceFieldName() => FolderPathSourceFieldName;

        public bool Resuming { get; set; }

        public int SyncStatisticsId { get; set; }

        public int BatchSizeForFileQueries { get; set; } = _BATCH_SIZE_FOR_FILE_QUERIES;

        public Guid? SnapshotId { get; set; }

        public string FileSizeColumn { get; set; }

        public string NativeFilePathSourceFieldName { get; set; }

        public string ImageFilePathSourceFieldName { get; set; }

        public string FileNameColumn { get; set; }

        public string OiFileTypeColumnName { get; set; }

        public string SupportedByViewerColumn { get; set; }

        public ImportOverwriteMode ImportOverwriteMode { get; set; }

        public FieldOverlayBehavior FieldOverlayBehavior { get; set; }

        public ImportNativeFileCopyMode ImportNativeFileCopyMode { get; set; }

        public int IdentityFieldId { get; set; }

        public DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; set; }

        public ImportNativeFileCopyMode NativeBehavior { get; }

        public bool ImageImport { get; set; }

        public ImportImageFileCopyMode ImportImageFileCopyMode { get; set; }

        public int[] ProductionImagePrecedence { get; set; } = { };

        public string GetSourceJobTagName() => _sourceJobTagName;

        public void SetSourceJobTagName(string sourceJobTagName)
        {
            _sourceJobTagName = sourceJobTagName;
        }

        public int SourceJobTagArtifactId { get; set; }

        public int SourceWorkspaceTagArtifactId { get; set; }

        public bool IsDestinationWorkspaceTagArtifactIdSet { get; set; }

        public async Task SetDestinationWorkspaceTagArtifactIdAsync(int artifactId)
        {
            await Task.Yield();
            DestinationWorkspaceTagArtifactId = artifactId;
            IsDestinationWorkspaceTagArtifactIdSet = true;
        }

        public int DestinationWorkspaceTagArtifactId { get; set; }

        public Guid JobHistoryObjectTypeGuid => JobHistory.TypeGuid;

        public int JobHistoryArtifactId { get; set; }

        Guid? IJobStatusConsolidationConfiguration.ExportRunId => _jobStatusConsolidationConfigurationExportRunId;

        public void SetJobStatusConsolidationConfigurationExportRunId(Guid? exportRunId)
        {
            _jobStatusConsolidationConfigurationExportRunId = exportRunId;
        }

        public bool IsSourceJobTagSet { get; set; }

        public Task SetSourceJobTagAsync(int artifactId, string name)
        {
            SourceJobTagArtifactId = artifactId;
            _sourceJobTagName = name;
            IsSourceJobTagSet = true;

            return Task.CompletedTask;
        }

        public async Task SetSourceWorkspaceTagAsync(int artifactId, string name)
        {
            await Task.Yield();
            SourceWorkspaceTagArtifactId = artifactId;
            SourceWorkspaceTagName = name;
        }

        public Task SetObjectLinkingSnapshotDataAsync(Guid objectLinkingSnapshotId, int objectLinkingRecordsCount)
        {
            ObjectLinkingSnapshotRecordsCount = objectLinkingRecordsCount;
            ObjectLinkingSnapshotId = objectLinkingSnapshotId;

            return Task.CompletedTask;
        }

        public Guid? ObjectLinkingSnapshotId { get; set; }

        public int ObjectLinkingSnapshotRecordsCount { get; set; }

        public int ExecutingUserId { get; set; } = _ADMIN_ID;

        public bool SendEmails { get; set; }

        public int TotalRecordsCount { get; set; }

        public Guid ExportRunId { get; set; }

        public int? JobHistoryToRetryId { get; set; }

        public string SyncVersion { get; set; }

        public bool IncludeOriginalImageIfNotFoundInProductions { get; set; }

        public bool IsImageJob { get; set; }

        public string IdentifierColumn { get; set; }

        public IJobHistoryRdoGuidsProvider JobHistory { get; set; } = DefaultGuids.JobHistory;

        public IJobHistoryStatusProvider JobHistoryStatus { get; set; } = DefaultGuids.JobHistoryStatus;

        public IJobHistoryErrorGuidsProvider JobHistoryError { get; set; } = DefaultGuids.JobHistoryError;

        public IDestinationWorkspaceTagGuidProvider DestinationWorkspace { get; set; } = DefaultGuids.DestinationWorkspace;

        public DataSourceType DataSourceType { get; set; }

        public DestinationLocationType DestinationType { get; set; }

        public string CorrelationId { get; set; }

        public string ExecutingApplication { get; set; }

        public string ExecutingApplicationVersion { get; set; }

        public bool LogItemLevelErrors { get; set; } = true;

        public bool LinkingExportExists => ObjectLinkingSnapshotId.HasValue;

        public int ImportApiBatchSize { get; set; } = 1000;

        public int SyncBatchSize { get; set; } = 25000;

        public bool IsPhysicalFileCopyMode { get; set; } = false;

        public Task<int> GetImportApiBatchSizeAsync()
        {
            return Task.FromResult(ImportApiBatchSize);
        }

        public Task<int> GetSyncBatchSizeAsync()
        {
            return Task.FromResult(SyncBatchSize);
        }
    }
}
