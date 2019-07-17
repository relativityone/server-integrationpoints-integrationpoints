namespace Relativity.Sync.Configuration
{
	internal interface IPermissionsCheckConfiguration : IConfiguration
	{
		int SourceWorkspaceArtifactId { get; }
		int DestinationWorkspaceArtifactId { get; }
		int DestinationFolderArtifactId { get;  }
		int IntegrationPointArtifactId { get; }
		int SourceProviderArtifactId { get; }
	}
}