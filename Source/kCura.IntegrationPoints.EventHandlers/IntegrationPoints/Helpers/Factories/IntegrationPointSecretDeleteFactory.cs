using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
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
			IRelativityObjectManager relativityObjectManager = CreateObjectManager(helper);
			IIntegrationPointRepository integrationPointRepository = new IntegrationPointRepository(relativityObjectManager);
			return new IntegrationPointSecretDelete(secretManager, secretCatalog, integrationPointRepository);
		}

		private static IRelativityObjectManager CreateObjectManager(IEHHelper helper)
		{
			return new RelativityObjectManagerFactory(helper).CreateRelativityObjectManager(helper.GetActiveCaseID());
		}
	}
}