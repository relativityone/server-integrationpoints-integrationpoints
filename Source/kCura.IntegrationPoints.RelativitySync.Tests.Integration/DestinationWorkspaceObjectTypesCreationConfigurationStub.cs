using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class DestinationWorkspaceObjectTypesCreationConfigurationStub : IDestinationWorkspaceObjectTypesCreationConfiguration
	{
		public bool IsSourceWorkspaceArtifactTypeIdSet { get; set; }
		public int SourceWorkspaceArtifactTypeId { get; set; }
		public bool IsSourceJobArtifactTypeIdSet { get; set; }
		public int SourceJobArtifactTypeId { get; set; }
		public int DestinationWorkspaceArtifactId { get; set; }
	}
}