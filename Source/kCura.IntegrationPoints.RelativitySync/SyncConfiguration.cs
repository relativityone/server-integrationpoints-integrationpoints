using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal sealed class SyncConfiguration : IDataDestinationFinalizationConfiguration, IDataDestinationInitializationConfiguration, IDataSourceSnapshotConfiguration,
		IDestinationWorkspaceObjectTypesCreationConfiguration, IDestinationWorkspaceSavedSearchCreationConfiguration, IDestinationWorkspaceTagsCreationConfiguration, IJobCleanupConfiguration,
		IJobStatusConsolidationConfiguration, INotificationConfiguration, IPermissionsCheckConfiguration, IPreviousRunCleanupConfiguration, ISnapshotPartitionConfiguration,
		ISourceWorkspaceTagsCreationConfiguration, ISynchronizationConfiguration, ITemporaryStorageInitializationConfiguration, IValidationConfiguration
	{
		private int? _jobTagArtifactId;
		private int? _workspaceTagArtifactId;
		private int? _savedSearchArtifactId;
		private int? _sourceWorkspaceArtifactTypeId;
		private int? _sourceJobArtifactTypeId;
		private int? _sourceJobTagArtifactId;
		private int? _sourceWorkspaceTagArtifactId;
		private string _sourceWorkspaceTagName;
		private int? _destinationWorkspaceTagArtifactId;

		public SyncConfiguration(int jobId, SourceConfiguration sourceConfiguration, ImportSettings destinationConfiguration, List<string> emailRecipients)
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
		}

		public string DataDestinationName => string.Empty;

		public bool IsDataDestinationArtifactIdSet => false;

		public int DataSourceArtifactId { get; }

		public int SnapshotId { get; set; } = 0;

		public int DataDestinationArtifactId { get; set; }

		public bool AreBatchesIdsSet => true;

		public List<int> BatchesIds { get; set; } = new List<int>();

		public int JobTagArtifactId
		{
			get
			{
				if (!_jobTagArtifactId.HasValue)
				{
					throw new ArgumentException($"Initialize {nameof(JobTagArtifactId)} first");
				}

				return _jobTagArtifactId.Value;
			}
			set => _jobTagArtifactId = value;
		}

		public int WorkspaceTagArtifactId
		{
			get
			{
				if (!_workspaceTagArtifactId.HasValue)
				{
					throw new ArgumentException($"Initialize {nameof(WorkspaceTagArtifactId)} first");
				}

				return _workspaceTagArtifactId.Value;
			}
			set => _workspaceTagArtifactId = value;
		}

		public bool CreateSavedSearchForTags { get; }

		public bool IsSavedSearchArtifactIdSet => _savedSearchArtifactId.HasValue;

		public bool IsStorageIdSet => true;

		public int StorageId { get; set; } = 0;

		public bool IsJobStatusArtifactIdSet => true;

		public int JobStatusArtifactId { get; set; }

		public bool IsPreviousRunArtifactIdSet { get; } = true;

		public int PreviousRunArtifactId { get; } = 0;

		public bool Retrying => false;

		public void SetSourceWorkspaceArtifactTypeId(int artifactTypeId)
		{
			_sourceWorkspaceArtifactTypeId = artifactTypeId;
		}

		public void SetSourceJobArtifactTypeId(int artifactTypeId)
		{
			_sourceJobArtifactTypeId = artifactTypeId;
		}

		public bool IsSourceWorkspaceArtifactTypeIdSet => _sourceWorkspaceArtifactTypeId.HasValue;

		public int JobArtifactId { get; }
		public bool IsDestinationWorkspaceTagArtifactIdSet => _destinationWorkspaceTagArtifactId.HasValue;
		public int SourceWorkspaceArtifactTypeId => _sourceWorkspaceArtifactTypeId.Value;
		public int SourceJobArtifactTypeId => _sourceJobArtifactTypeId.Value;

		public bool IsSourceJobArtifactTypeIdSet => _sourceJobArtifactTypeId.HasValue;

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
			_sourceWorkspaceTagName = name;
		}

		public int SourceWorkspaceArtifactId { get; }

		public void SetDestinationWorkspaceTagArtifactId(int artifactId)
		{
			_destinationWorkspaceTagArtifactId = artifactId;
		}

		public void SetSavedSearchArtifactId(int artifactId)
		{
			_savedSearchArtifactId = artifactId;
		}

		public int DestinationWorkspaceArtifactId { get; }
		public string SourceJobTagName { get; private set; }

		public int SourceJobTagArtifactId => _sourceJobTagArtifactId.Value;
		public int SourceWorkspaceTagArtifactId => _sourceWorkspaceTagArtifactId.Value;
		public string JobStatus { get; set; }
		public bool SendEmails { get; }
		public IEnumerable<string> EmailRecipients { get; }
	}
}