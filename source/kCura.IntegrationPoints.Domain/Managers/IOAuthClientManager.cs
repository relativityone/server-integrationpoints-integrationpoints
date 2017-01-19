using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Domain.Managers
{
	public interface IOAuthClientManager
	{
		OAuthClientDto RetrieveOAuthClientForFederatedInstance(int federatedInstanceArtifactId);
	}
}