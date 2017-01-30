using kCura.IntegrationPoints.Data;
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
			ISecretManager secretManager = new SecretManager();
			ISecretCatalog secretCatalog = new DefaultSecretCatalogFactory().Create(helper.GetActiveCaseID());
			return new IntegrationPointSecretDelete(secretManager, secretCatalog, new RsapiClientLibrary<Data.IntegrationPoint>(helper, helper.GetActiveCaseID()));
		}
	}
}