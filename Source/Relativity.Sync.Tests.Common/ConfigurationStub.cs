using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Common
{
	[ExcludeFromCodeCoverage]
	internal sealed class ConfigurationStub : IDataDestinationFinalizationConfiguration, IDataDestinationInitializationConfiguration, IDataSourceSnapshotConfiguration,
		IDestinationWorkspaceObjectTypesCreationConfiguration, IDestinationWorkspaceSavedSearchCreationConfiguration, IDestinationWorkspaceTagsCreationConfiguration, IJobCleanupConfiguration,
		IJobStatusConsolidationConfiguration, INotificationConfiguration, IPermissionsCheckConfiguration, ISnapshotPartitionConfiguration,
		ISourceWorkspaceTagsCreationConfiguration, ISynchronizationConfiguration, IValidationConfiguration, IUserContextConfiguration
	{
		private const int _ADMIN_ID = 9;

		public string DataDestinationName { get; set; }
		public bool IsDataDestinationArtifactIdSet { get; set; }
		public int DataDestinationArtifactId { get; set; }
		public int DataSourceArtifactId { get; set; }
		public IList<FieldMap> FieldMappings { get; set; }
		public bool IsSnapshotCreated { get; set; }
		
		public async Task SetSnapshotDataAsync(Guid runId, long totalRecordsCount)
		{
			await Task.Yield();
			ExportRunId = runId;
			TotalRecordsCount = (int) totalRecordsCount;
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


		public int ExecutingUserId => _ADMIN_ID;
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