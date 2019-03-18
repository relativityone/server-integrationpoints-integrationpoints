using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers.Implementations;

namespace kCura.IntegrationPoints.EventHandlers.Installers.Helpers.Factories
{
	public static class SecretStoreCleanUpFactory
	{
		public static ISecretStoreCleanUp Create(int workspaceArtifactId)
		{
			var secretManager = new SecretManager(workspaceArtifactId);
			var secretCatalog = new DefaultSecretCatalogFactory().Create(workspaceArtifactId);
			return new SecretStoreCleanUp(secretManager, secretCatalog);
		}
	}
}