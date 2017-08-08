using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Data
{
	public interface IRsapiClientFactory
	{
		IRSAPIClient CreateAdminClient(int workspaceArtifactId);
		IRSAPIClient CreateUserClient(int workspaceArtifactId);
	}
}