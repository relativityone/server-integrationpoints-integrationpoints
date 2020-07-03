#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Data
{
	public interface IRsapiClientWithWorkspaceFactory
	{
		IRSAPIClient CreateAdminClient(int workspaceArtifactId = -1);
		IRSAPIClient CreateUserClient(int workspaceArtifactId);
	}
}
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
