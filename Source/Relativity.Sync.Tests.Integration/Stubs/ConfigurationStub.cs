using System.Collections.Generic;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	internal sealed class ConfigurationStub : IDataDestinationFinalizationConfiguration, IDataDestinationInitializationConfiguration, IDataSourceSnapshotConfiguration,
		IDestinationWorkspaceObjectTypesCreationConfiguration, IDestinationWorkspaceSavedSearchCreationConfiguration, IDestinationWorkspaceTagsCreationConfiguration, IJobCleanupConfiguration,
		IJobStatusConsolidationConfiguration, INotificationConfiguration, IPermissionsCheckConfiguration, IPreviousRunCleanupConfiguration, ISnapshotPartitionConfiguration,
		ISourceWorkspaceTagsCreationConfiguration, ISynchronizationConfiguration, ITemporaryStorageInitializationConfiguration, IValidationConfiguration
	{
		public string DataDestinationName { get; set; }
		public bool IsDataDestinationArtifactIdSet { get; set; }
		public int DataDestinationArtifactId { get; set; }
		public int DataSourceArtifactId { get; set; }
		public int SnapshotId { get; set; }
		public bool AreBatchesIdsSet { get; set; }
		public List<int> BatchesIds { get; set; }
		public int JobTagArtifactId { get; set; }
		public bool IsWorkspaceTagArtifactIdSet { get; set; }
		public bool IsJobTagArtifactIdSet { get; set; }
		public int WorkspaceTagArtifactId { get; set; }
		public bool IsSavedSearchArtifactIdSet { get; set; }
		public int SavedSearchArtifactId { get; set; }
		public bool IsStorageIdSet { get; set; }
		public int StorageId { get; set; }
		public bool IsJobStatusArtifactIdSet { get; set; }
		public int JobStatusArtifactId { get; set; }
		public bool IsPreviousRunArtifactIdSet { get; set; }
		public int PreviousRunArtifactId { get; set; }
		public bool Retrying { get; set; }
		public bool IsTagArtifactIdSet { get; set; }
		public int TagArtifactId { get; set; }
		public bool IsSourceWorkspaceArtifactTypeIdSet { get; set; }
		public int SourceWorkspaceArtifactTypeId { get; set; }
		public bool IsSourceJobArtifactTypeIdSet { get; set; }
		public int SourceJobArtifactTypeId { get; set; }
		public int DestinationWorkspaceArtifactId { get; set; }
	}
}