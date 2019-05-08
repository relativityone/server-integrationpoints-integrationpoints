using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal sealed class SyncConfiguration : IDataDestinationFinalizationConfiguration, IDataDestinationInitializationConfiguration, IDataSourceSnapshotConfiguration,
		IDestinationWorkspaceObjectTypesCreationConfiguration, IDestinationWorkspaceSavedSearchCreationConfiguration, IDestinationWorkspaceTagsCreationConfiguration, IJobCleanupConfiguration,
		IJobStatusConsolidationConfiguration, INotificationConfiguration, IPermissionsCheckConfiguration, ISnapshotPartitionConfiguration,
		ISourceWorkspaceTagsCreationConfiguration, ISynchronizationConfiguration, IValidationConfiguration, IUserContextConfiguration
	{
		private int? _savedSearchArtifactId;
		private int? _sourceJobTagArtifactId;
		private int? _sourceWorkspaceTagArtifactId;
		private int? _destinationWorkspaceTagArtifactId;
		private Guid? _exportRunId;
		private List<int> _batchesIds;

		public SyncConfiguration(int jobId, int submittedBy, SourceConfiguration sourceConfiguration, ImportSettings destinationConfiguration, List<string> emailRecipients)
		{
			JobStatusArtifactId = jobId;
			DataSourceArtifactId = sourceConfiguration.SavedSearchArtifactId;
			DataDestinationArtifactId = destinationConfiguration.DestinationFolderArtifactId;
			DestinationWorkspaceArtifactId = destinationConfiguration.CaseArtifactId;
			JobArtifactId = jobId;
			SourceWorkspaceArtifactId = sourceConfiguration.SourceWorkspaceArtifactId;
			CreateSavedSearchForTags = destinationConfiguration.CreateSavedSearchForTagging;
			EmailRecipients = emailRecipients;
			SendEmails = emailRecipients.Count > 0;
			ExecutingUserId = submittedBy;
		}

		public string DataDestinationName => string.Empty;

		public bool IsDataDestinationArtifactIdSet => false;

		public int DataSourceArtifactId { get; }

		public bool IsSnapshotCreated => _exportRunId.HasValue;

		public int DataDestinationArtifactId { get; set; }

		public bool CreateSavedSearchForTags { get; }

		public bool IsSavedSearchArtifactIdSet => _savedSearchArtifactId.HasValue;

		public int JobStatusArtifactId { get; set; }

		public bool Retrying => false;

		public int JobArtifactId { get; }
		public int SourceWorkspaceArtifactTypeId { get; set; }

		public bool IsDestinationWorkspaceTagArtifactIdSet => _destinationWorkspaceTagArtifactId.HasValue;
		public int DestinationWorkspaceTagArtifactId => _destinationWorkspaceTagArtifactId.Value;
		public int JobHistoryTagArtifactId => _sourceJobTagArtifactId.Value;

		public bool IsSourceJobTagSet => _sourceJobTagArtifactId.HasValue;
		public bool IsSourceWorkspaceTagSet => _sourceWorkspaceTagArtifactId.HasValue;

		public void SetSourceJobTag(int artifactId, string name)
		{
			_sourceJobTagArtifactId = artifactId;
			SourceJobTagName = name;
		}

		public void SetSourceWorkspaceTag(int artifactId, string name)
		{
			_sourceWorkspaceTagArtifactId = artifactId;
			SourceWorkspaceTagName = name;
		}

		public string JobName { get; set; }
		public string NotificationEmails { get; set; }
		public int SourceWorkspaceArtifactId { get; }
		public int SyncConfigurationArtifactId { get; }

		public void SetDestinationWorkspaceTagArtifactId(int artifactId)
		{
			_destinationWorkspaceTagArtifactId = artifactId;
		}

		public Task SetSavedSearchInDestinationArtifactIdAsync(int artifactId)
		{
			_savedSearchArtifactId = artifactId;
			return Task.CompletedTask;
		}

		public int DestinationWorkspaceArtifactId { get; }
		public int SavedSearchArtifactId => _savedSearchArtifactId.Value;
		public int DestinationFolderArtifactId { get; set; }
		public int FolderPathSourceFieldArtifactId { get; set; }
		public ImportOverwriteMode ImportOverwriteMode { get; set; }
		public FieldOverlayBehavior FieldOverlayBehavior { get; set; }
		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; set; }
		public string SourceJobTagName { get; private set; }
		public string SourceWorkspaceTagName { get; private set; }

		public int SourceJobTagArtifactId => _sourceJobTagArtifactId.Value;
		public int SourceWorkspaceTagArtifactId => _sourceWorkspaceTagArtifactId.Value;
		public string JobStatus { get; set; }
		public bool SendEmails { get; }
		public IEnumerable<string> EmailRecipients { get; }

		public bool IsSnapshotPartitioned => _batchesIds != null;

		public void SetSnapshotPartitions(List<int> batchesIds)
		{
			_batchesIds = batchesIds;
		}

		public int TotalRecordsCount { get; private set; }
		public int BatchSize => int.MaxValue;

		public Guid ExportRunId => _exportRunId.Value;

		public int ExecutingUserId { get; private set; }

		public Task SetSnapshotDataAsync(Guid runId, long totalRecordsCount)
		{
			return Task.CompletedTask;
		}

		public IList<FieldMap> FieldMappings { get; }
	}
}