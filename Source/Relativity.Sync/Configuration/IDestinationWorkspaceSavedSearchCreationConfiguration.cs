namespace Relativity.Sync.Configuration
{
	internal interface IDestinationWorkspaceSavedSearchCreationConfiguration : IConfiguration
	{
		int JobTagArtifactId { get; }

		int WorkspaceTagArtifactId { get; }

		//params to create saved search name

		bool IsSavedSearchArtifactIdSet { get; }

		int SavedSearchArtifactId { get; set; }
	}
}