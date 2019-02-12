using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration.Stubs
{
	internal sealed class SourceWorkspaceTagsCreationConfigurationStub : ISourceWorkspaceTagsCreationConfiguration
	{
		public void SetDestinationWorkspaceTagArtifactId(int artifactId)
		{
			DestinationWorkspaceTagArtifactId = artifactId;
			IsDestinationWorkspaceTagArtifactIdSet = true;
		}

		public int DestinationWorkspaceTagArtifactId { get; private set; }
		public int DestinationWorkspaceArtifactId { get; set; }
		public int SourceWorkspaceArtifactId { get; set; }
		public int JobArtifactId { get; set; }
		public bool IsDestinationWorkspaceTagArtifactIdSet { get; private set; }
	}
}