namespace Relativity.Sync.Configuration
{
	internal interface IDestinationWorkspaceTagsCreationConfiguration : IConfiguration
	{
		//fields from RelativitySourceCase and RelativitySourceJob

		bool IsWorkspaceTagArtifactIdSet { get; }

		bool IsJobTagArtifactIdSet { get; }

		int WorkspaceTagArtifactId { get; set; }

		int JobTagArtifactId { get; set; }
	}
}