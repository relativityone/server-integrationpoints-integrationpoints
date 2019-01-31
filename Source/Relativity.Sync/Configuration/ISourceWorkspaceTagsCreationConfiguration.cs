namespace Relativity.Sync.Configuration
{
	internal interface ISourceWorkspaceTagsCreationConfiguration : IConfiguration
	{
		int DestinationWorkspaceArtifactId { get; }

		int SourceWorkspaceArtifactId { get; }

		int JobArtifactId { get; }

		bool IsDestinationWorkspaceTagArtifactIdSet { get; }

		void SetDestinationWorkspaceTagArtifactId(int artifactId);
	}
}