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
		private int? _tagArtifactId;
		private int? _sourceWorkspaceArtifactTypeId;
		private int? _sourceJobArtifactTypeId;
		private int? _sourceJobTagArtifactId;
		private string _sourceJobTagName;
		private int? _sourceWorkspaceTagArtifactId;
		private string _sourceWorkspaceTagName;

		public SyncConfiguration(int jobId, SourceConfiguration sourceConfiguration, ImportSettings destinationConfiguration)
		{
			JobStatusArtifactId = jobId;
			DataSourceArtifactId = sourceConfiguration.SavedSearchArtifactId;
			DataDestinationArtifactId = destinationConfiguration.DestinationFolderArtifactId;
			DestinationWorkspaceArtifactId = destinationConfiguration.CaseArtifactId;
			JobArtifactId = jobId;
			SourceWorkspaceArtifactId = sourceConfiguration.SourceWorkspaceArtifactId;
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

		public bool IsSavedSearchArtifactIdSet => _savedSearchArtifactId.HasValue;

		public int SavedSearchArtifactId
		{
			get
			{
				if (!_savedSearchArtifactId.HasValue)
				{
					throw new ArgumentException($"Initialize {nameof(SavedSearchArtifactId)} first");
				}

				return _savedSearchArtifactId.Value;
			}
			set => _savedSearchArtifactId = value;
		}

		public bool IsStorageIdSet => true;

		public int StorageId { get; set; } = 0;

		public bool IsJobStatusArtifactIdSet => true;

		public int JobStatusArtifactId { get; set; }

		public bool IsPreviousRunArtifactIdSet { get; } = true;

		public int PreviousRunArtifactId { get; } = 0;

		public bool Retrying => false;

		public bool IsTagArtifactIdSet => _tagArtifactId.HasValue;

		public int TagArtifactId
		{
			get
			{
				if (!_tagArtifactId.HasValue)
				{
					throw new ArgumentException($"Initialize {nameof(TagArtifactId)} first");
				}

				return _tagArtifactId.Value;
			}
			set => _tagArtifactId = value;
		}

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
		public int SourceWorkspaceArtifactTypeId => _sourceWorkspaceArtifactTypeId.Value;
		public int SourceJobArtifactTypeId => _sourceJobArtifactTypeId.Value;

		public bool IsSourceJobArtifactTypeIdSet => _sourceJobArtifactTypeId.HasValue;

		public bool IsSourceJobTagSet => _sourceJobTagArtifactId.HasValue;
		public bool IsSourceWorkspaceTagSet => _sourceWorkspaceTagArtifactId.HasValue;

		public void SetSourceJobTag(int artifactId, string name)
		{
			_sourceJobTagArtifactId = artifactId;
			_sourceJobTagName = name;
		}

		public void SetSourceWorkspaceTag(int artifactId, string name)
		{
			_sourceWorkspaceTagArtifactId = artifactId;
			_sourceWorkspaceTagName = name;
		}

		public int SourceWorkspaceArtifactId { get; }
		public int DestinationWorkspaceArtifactId { get; }
	}
}