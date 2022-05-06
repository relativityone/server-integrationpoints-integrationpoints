using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories
{
    public static class RelativityProviderSourceConfigurationFactory
	{
		public static IRelativityProviderConfiguration Create(IEHHelper helper, IInstanceSettingsManager federatedInstanceManager)
		{
			IAPILog logger = helper.GetLoggerFactory().GetLogger();

			IManagerFactory managerFactory = new ManagerFactory(helper, new FakeNonRemovableAgent());
			IRepositoryFactory repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());
			IProductionManager productionManager = new ProductionManager(repositoryFactory, logger);

			return new RelativityProviderSourceConfiguration(helper, productionManager, managerFactory, federatedInstanceManager);
		}
	}
}