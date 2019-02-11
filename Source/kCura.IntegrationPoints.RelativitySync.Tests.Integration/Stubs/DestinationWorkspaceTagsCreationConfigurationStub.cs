using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration.Stubs
{
	internal sealed class DestinationWorkspaceTagsCreationConfigurationStub : IDestinationWorkspaceTagsCreationConfiguration
	{
		public int? SourceJobTagArtifactId { get; private set; }
		public string SourceJobTagName { get; private set; }
		public int? SourceWorkspaceTagArtifactId { get; private set; }
		public string SourceWorkspaceTagName { get; private set; }

		public void SetSourceJobTag(int artifactId, string name)
		{
			SourceJobTagArtifactId = artifactId;
			SourceJobTagName = name;
		}

		public void SetSourceWorkspaceTag(int artifactId, string name)
		{
			SourceWorkspaceTagArtifactId = artifactId;
			SourceWorkspaceTagName = name;
		}

		public int SourceWorkspaceArtifactId { get; set; }
		public int DestinationWorkspaceArtifactId { get; set; }
		public int JobArtifactId { get; set; }
		public int SourceWorkspaceArtifactTypeId { get; set; }
		public int SourceJobArtifactTypeId { get; set; }
		public bool IsSourceJobTagSet => SourceJobTagArtifactId.HasValue;
		public bool IsSourceWorkspaceTagSet => SourceWorkspaceTagArtifactId.HasValue;
	}
}