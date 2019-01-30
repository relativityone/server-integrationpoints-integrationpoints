namespace Relativity.Sync.Configuration
{
	internal interface IDestinationWorkspaceObjectTypesCreationConfiguration : IConfiguration
	{
		bool IsSourceWorkspaceArtifactTypeIdSet { get; }

		void SetSourceWorkspaceArtifactTypeId(int artifactTypeId);

		bool IsSourceJobArtifactTypeIdSet { get; }

		void SetSourceJobArtifactTypeId(int artifactTypeId);

		int DestinationWorkspaceArtifactId { get; }
	}
}