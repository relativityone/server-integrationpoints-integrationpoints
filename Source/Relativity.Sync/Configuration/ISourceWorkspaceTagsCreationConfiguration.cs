namespace Relativity.Sync.Configuration
{
	internal interface ISourceWorkspaceTagsCreationConfiguration : IConfiguration
	{
		//fields from DestinationWorkspace object

		bool IsTagArtifactIdSet { get; }

		int TagArtifactId { get; set; }
	}
}