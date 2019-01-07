namespace Relativity.Sync.Configuration
{
	internal interface IDestinationWorkspaceTagsCreationConfiguration : IConfiguration
	{
		//TODO fields from RelativitySourceCase and RelativitySourceJob

		bool IsWorkspaceTagArtifactIdSet { get; }

		bool IsJobTagArtifactIdSet { get; }

		int WorkspaceTagArtifactId { set; }

		int JobTagArtifactId { set; }
	}
}