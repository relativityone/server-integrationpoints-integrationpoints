namespace Relativity.Sync.Configuration
{
	internal interface IDestinationWorkspaceSavedSearchCreationConfiguration : IConfiguration
	{
		int DestinationWorkspaceArtifactId { get; }

		string SourceJobTagName { get; }

		int SourceJobTagArtifactId { get; }

		int SourceWorkspaceTagArtifactId { get; }

		bool CreateSavedSearchForTags { get; }

		bool IsSavedSearchArtifactIdSet { get; }

		void SetSavedSearchArtifactId(int artifactId);
	}
}