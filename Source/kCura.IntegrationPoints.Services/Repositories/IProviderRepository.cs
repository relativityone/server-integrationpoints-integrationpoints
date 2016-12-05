namespace kCura.IntegrationPoints.Services.Repositories
{
	public interface IProviderRepository
	{
		int GetDestinationProviderArtifactId(int workspaceArtifactId, string destinationProviderGuidIdentifier);
		int GetSourceProviderArtifactId(int workspaceArtifactId, string sourceProviderGuidIdentifier);
	}
}