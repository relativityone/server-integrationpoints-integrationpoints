using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories
{
	public class IntegrationPointSecretDeleteFactory
	{
		public static IIntegrationPointSecretDelete Create(IEHHelper helper)
		{
			ISecretManager secretManager = new SecretManager(helper.GetActiveCaseID());
			ISecretCatalog secretCatalog = new DefaultSecretCatalogFactory().Create(helper.GetActiveCaseID());
			return new IntegrationPointSecretDelete(secretManager, 
				secretCatalog, 
				new RelativityObjectManager(helper.GetActiveCaseID(), 
				helper, new SecretStoreHelper(helper.GetActiveCaseID(),
						helper,
						new SecretManager(helper.GetActiveCaseID()),
						new DefaultSecretCatalogFactory())));
		}
	}
}