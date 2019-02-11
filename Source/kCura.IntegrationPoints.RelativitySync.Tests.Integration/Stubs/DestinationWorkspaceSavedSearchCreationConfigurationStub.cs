using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration.Stubs
{
	internal sealed class DestinationWorkspaceSavedSearchCreationConfigurationStub : IDestinationWorkspaceSavedSearchCreationConfiguration
	{
		public void SetSavedSearchArtifactId(int artifactId)
		{
			SavedSearchArtifactId = artifactId;
			IsSavedSearchArtifactIdSet = true;
		}

		public int SavedSearchArtifactId { get; private set; }
		public int DestinationWorkspaceArtifactId { get; set; }
		public string SourceJobTagName { get; set; }
		public int SourceJobTagArtifactId { get; set; }
		public int SourceWorkspaceTagArtifactId { get; set; }
		public bool CreateSavedSearchForTags { get; set; }
		public bool IsSavedSearchArtifactIdSet { get; private set; }
	}
}