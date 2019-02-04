using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class DestinationWorkspaceObjectTypesCreationConfigurationStub : IDestinationWorkspaceObjectTypesCreationConfiguration
	{
		public void SetSourceWorkspaceArtifactTypeId(int artifactTypeId)
		{
			SourceWorkspaceArtifactTypeId = artifactTypeId;
		}

		public void SetSourceJobArtifactTypeId(int artifactTypeId)
		{
			SourceJobArtifactTypeId = artifactTypeId;
		}

		public int SourceWorkspaceArtifactTypeId { get; private set; }
		public int SourceJobArtifactTypeId { get; private set; }
		public bool IsSourceWorkspaceArtifactTypeIdSet { get; set; }
		public bool IsSourceJobArtifactTypeIdSet { get; set; }
		public int DestinationWorkspaceArtifactId { get; set; }
	}
}