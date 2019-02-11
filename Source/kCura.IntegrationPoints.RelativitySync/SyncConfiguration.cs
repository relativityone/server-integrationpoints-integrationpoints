﻿using System;
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

		public SyncConfiguration(int jobId, SourceConfiguration sourceConfiguration, ImportSettings destinationConfiguration)
		{
			JobStatusArtifactId = jobId;
			DataSourceArtifactId = sourceConfiguration.SavedSearchArtifactId;
			DataDestinationArtifactId = destinationConfiguration.DestinationFolderArtifactId;
			DestinationWorkspaceArtifactId = destinationConfiguration.CaseArtifactId;
		}

		public string DataDestinationName => string.Empty;

		public bool IsDataDestinationArtifactIdSet => false;

		public int DataSourceArtifactId { get; }

		public int SnapshotId { get; set; } = 0;

		public int DataDestinationArtifactId { get; set; }

		public bool AreBatchesIdsSet => true;

		public List<int> BatchesIds { get; set; } = new List<int>();

		public bool IsJobTagArtifactIdSet => _jobTagArtifactId.HasValue;

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

		public bool IsWorkspaceTagArtifactIdSet => _workspaceTagArtifactId.HasValue;

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

		public void SetDestinationWorkspaceTagArtifactId(int artifactId)
		{
			throw new NotImplementedException();
		}

		public void SetSourceJobTag(int artifactId, string name)
		{
			throw new NotImplementedException();
		}

		public void SetSourceWorkspaceTag(int artifactId, string name)
		{
			throw new NotImplementedException();
		}

		public bool IsSourceWorkspaceArtifactTypeIdSet => _sourceWorkspaceArtifactTypeId.HasValue;

		public bool IsSourceJobArtifactTypeIdSet => _sourceJobArtifactTypeId.HasValue;

		public int DestinationWorkspaceArtifactId { get; }

		public int ExecutingUserId { get; } = -1;

		public int SourceWorkspaceArtifactId => throw new NotImplementedException();

		public int JobArtifactId => throw new NotImplementedException();

		public bool IsDestinationWorkspaceTagArtifactIdSet => throw new NotImplementedException();

		public int SourceWorkspaceArtifactTypeId => throw new NotImplementedException();

		public int SourceJobArtifactTypeId => throw new NotImplementedException();

		public bool IsSourceJobTagSet => throw new NotImplementedException();

		public bool IsSourceWorkspaceTagSet => throw new NotImplementedException();
	}
}