using System;
using System.Collections.Generic;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	internal sealed class ConfigurationStub : IDataDestinationFinalizationConfiguration, IDataDestinationInitializationConfiguration, IDataSourceSnapshotConfiguration,
		IDestinationWorkspaceObjectTypesCreationConfiguration, IDestinationWorkspaceSavedSearchCreationConfiguration, IDestinationWorkspaceTagsCreationConfiguration, IJobCleanupConfiguration,
		IJobStatusConsolidationConfiguration, INotificationConfiguration, IPermissionsCheckConfiguration, ISnapshotPartitionConfiguration,
		ISourceWorkspaceTagsCreationConfiguration, ISynchronizationConfiguration, IValidationConfiguration, IUserContextConfiguration
	{
		public string DataDestinationName { get; set; }
		public bool IsDataDestinationArtifactIdSet { get; set; }
		public int DataDestinationArtifactId { get; set; }
		public int DataSourceArtifactId { get; set; }
		public string FieldMappings { get; set; }
		public bool IsSnapshotCreated { get; set; }

		public void SetSnapshotData(Guid runId, int totalRecordsCount)
		{
			// Method intentionally left empty.
		}

		public bool CreateSavedSearchForTags { get; set; }
		public bool IsSavedSearchArtifactIdSet { get; set; }
		public void SetSavedSearchArtifactId(int artifactId)
		{
			// Method intentionally left empty.
		}
		public bool IsSourceWorkspaceArtifactTypeIdSet { get; set; }

		public void SetSourceWorkspaceArtifactTypeId(int artifactTypeId)
		{
			// Method intentionally left empty.
		}

		public bool IsSourceJobArtifactTypeIdSet { get; set; }

		public void SetSourceJobArtifactTypeId(int artifactTypeId)
		{
			// Method intentionally left empty.
		}

		public int SourceWorkspaceArtifactId { get; set; }
		public int DestinationWorkspaceArtifactId { get; set; }
		public string SourceJobTagName { get; set; }
		public int SourceJobTagArtifactId { get; set; }
		public int SourceWorkspaceTagArtifactId { get; set; }
		public int JobArtifactId { get; set; }
		public bool IsDestinationWorkspaceTagArtifactIdSet { get; set; }
		public void SetDestinationWorkspaceTagArtifactId(int artifactId)
		{
			DestinationWorkspaceTagArtifactId = artifactId;
		}
		public int DestinationWorkspaceTagArtifactId { get; set; }

		public int SourceWorkspaceArtifactTypeId { get; set; }
		public int SourceJobArtifactTypeId { get; set; }
		public bool IsSourceJobTagSet { get; set; }

		public void SetSourceJobTag(int artifactId, string name)
		{
			// Method intentionally left empty.
		}

		public bool IsSourceWorkspaceTagSet { get; set; }

		public void SetSourceWorkspaceTag(int artifactId, string name)
		{
		}


		public int ExecutingUserId { get; set; }
		public string JobStatus { get; set; }
		public bool SendEmails { get; set; }
		public IEnumerable<string> EmailRecipients { get; } = new List<string>();
		public int TotalRecordsCount { get; set; }
		public Guid ExportRunId { get; set; }
		public bool IsSnapshotPartitioned { get; set; }
		public void SetSnapshotPartitions(List<int> batchesIds)
		{
			// Method intentionally left empty.
		}
	}
}