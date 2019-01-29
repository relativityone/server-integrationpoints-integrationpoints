namespace Relativity.Sync.Configuration
{
	internal interface IDestinationWorkspaceObjectTypesCreationConfiguration : IConfiguration
	{
		bool IsSourceWorkspaceArtifactTypeIdSet { get; }

		int SourceWorkspaceArtifactTypeId { get; set; }

		bool IsSourceJobArtifactTypeIdSet { get; }

		int SourceJobArtifactTypeId { get; set; }

		int DestinationWorkspaceArtifactId { get; }
	}
}