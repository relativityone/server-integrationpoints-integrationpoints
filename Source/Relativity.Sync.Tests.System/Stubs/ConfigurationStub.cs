using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Tests.System.Stubs
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

		public async Task SetSnapshotDataAsync(Guid runId, long totalRecordsCount)
		{
			await Task.Yield();
		}

		public string SourceWorkspaceTagName { get; set; }
		public bool CreateSavedSearchForTags { get; set; }
		public bool IsSavedSearchArtifactIdSet { get; set; }
		public async Task SetSavedSearchInDestinationArtifactIdAsync(int artifactId)
		{
			await Task.Yield();
		}

		public string JobName { get; set; }
		public string NotificationEmails { get; set; }
		public int SourceWorkspaceArtifactId { get; set; }
		public int DestinationWorkspaceArtifactId { get; set; }
		public int SavedSearchArtifactId { get; set; }
		public int DestinationFolderArtifactId { get; set; }
		public int FolderPathSourceFieldArtifactId { get; set; }
		public ImportOverwriteMode ImportOverwriteMode { get; set; }
		public FieldOverlayBehavior FieldOverlayBehavior { get; set; }
		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; set; }
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
			SourceJobTagArtifactId = artifactId;
			SourceJobTagName = name;
		}

		public bool IsSourceWorkspaceTagSet { get; set; }

		public void SetSourceWorkspaceTag(int artifactId, string name)
		{
			SourceWorkspaceTagArtifactId = artifactId;
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