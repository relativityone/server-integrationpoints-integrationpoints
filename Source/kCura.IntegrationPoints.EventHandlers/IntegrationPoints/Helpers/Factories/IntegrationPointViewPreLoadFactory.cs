using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories
{
	public static class IntegrationPointViewPreLoadFactory
	{
		public static IIntegrationPointViewPreLoad Create(IEHHelper helper, IIntegrationPointBaseFieldsConstants integrationPointBaseFieldsConstants)
		{
			ICaseServiceContext caseServiceContext = ServiceContextFactory.CreateCaseServiceContext(helper, helper.GetActiveCaseID());
			IAPILog logger = helper.GetLoggerFactory().GetLogger();
			IIntegrationPointSerializer integrationPointSerializer = new IntegrationPointSerializer(logger);
			ISecretsRepository secretsRepository = new SecretsRepository(
				SecretStoreFacadeFactory_Deprecated.Create(helper.GetSecretStore, logger), 
				logger
			);
			IIntegrationPointRepository integrationPointRepository = new IntegrationPointRepository(
				caseServiceContext.RsapiService.RelativityObjectManager,
				integrationPointSerializer,
				secretsRepository,
				logger);

			IRepositoryFactory repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());
			Domain.Managers.IFederatedInstanceManager federatedInstanceManager = new FederatedInstanceManager(repositoryFactory);
			Domain.Managers.IInstanceSettingsManager instanceSettingsManager =new InstanceSettingsManager(repositoryFactory);
			IRelativityProviderConfiguration relativityProviderSourceConfiguration =
				RelativityProviderSourceConfigurationFactory.Create(helper, instanceSettingsManager);

			IRelativityProviderConfiguration relativityProviderDestinationConfiguration =
				new RelativityProviderDestinationConfiguration(helper, federatedInstanceManager, repositoryFactory);

			return new IntegrationPointViewPreLoad(
				caseServiceContext,
				relativityProviderSourceConfiguration,
				relativityProviderDestinationConfiguration,
				integrationPointBaseFieldsConstants);
		}
	}
}