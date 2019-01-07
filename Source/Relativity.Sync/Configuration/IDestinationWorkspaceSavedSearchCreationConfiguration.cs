namespace Relativity.Sync.Configuration
{
	internal interface IDestinationWorkspaceSavedSearchCreationConfiguration : IConfiguration
	{
		int JobTagArtifactId { get; }

		int WorkspaceTagArtifactId { get; }

		//TODO params to create saved search name

		bool IsSavedSearchArtifactIdSet { get; }

		int SavedSearchArtifactId { set; }
	}
}