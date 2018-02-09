using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Data
{
	public interface IRsapiClientWithWorkspaceFactory
	{
		IRSAPIClient CreateAdminClient(int workspaceArtifactId = -1);
		IRSAPIClient CreateUserClient(int workspaceArtifactId);
	}
}