namespace Relativity.Sync.Configuration
{
	internal interface ISourceWorkspaceTagsCreationConfiguration : IConfiguration
	{
		//TODO fields from DestinationWorkspace object

		bool IsTagArtifactIdSet { get; }

		int TagArtifactId { set; }
	}
}