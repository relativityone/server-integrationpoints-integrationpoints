namespace kCura.IntegrationPoints.Data.Factories
{
	public interface IRSAPIServiceFactory
	{
		IRSAPIService Create(int workspaceArtifactId);
	}
}