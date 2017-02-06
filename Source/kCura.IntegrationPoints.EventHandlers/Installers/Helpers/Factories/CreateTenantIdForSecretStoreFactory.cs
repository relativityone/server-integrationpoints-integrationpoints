using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers.Implementations;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.EventHandlers.Installers.Helpers.Factories
{
	public class CreateTenantIdForSecretStoreFactory
	{
		public static ICreateTenantIdForSecretStore Create(int workspaceArtifactId)
		{
			ISecretManager secretManager = new SecretManager(workspaceArtifactId);
			ISecretCatalog secretCatalog = new DefaultSecretCatalogFactory().Create(workspaceArtifactId);
			return new CreateTenantIdForSecretStore(secretCatalog, secretManager);
		}
	}
}