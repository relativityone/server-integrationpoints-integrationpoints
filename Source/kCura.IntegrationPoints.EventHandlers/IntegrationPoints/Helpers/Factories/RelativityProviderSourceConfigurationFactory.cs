using System;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Authentication.WebApi;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.ScheduleQueue.Core.Data;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories
{
	public static class RelativityProviderSourceConfigurationFactory
	{
		public static IRelativityProviderConfiguration Create(IEHHelper helper, IInstanceSettingsManager federatedInstanceManager)
		{
			IConfigFactory configFactory = new ConfigFactory();
			IAPILog logger = helper.GetLoggerFactory().GetLogger();
			IWebApiLoginService credentialProvider = WebApiLoginServiceFactoryDeprecated.Create(logger);
			ISqlServiceFactory sqlServiceFactory = new HelperConfigSqlServiceFactory(helper);
			IServiceManagerProvider serviceManagerProvider = new ServiceManagerProvider(configFactory, credentialProvider, sqlServiceFactory);
			IQueueQueryManager queryManager = new QueueQueryManager(helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
			IJobServiceDataProvider jobServiceDataProvider = new JobServiceDataProvider(queryManager);
			IToggleProvider toggleProvider = ToggleProvider.Current;

			IManagerFactory managerFactory = new ManagerFactory(helper, new FakeNonRemovableAgent(), jobServiceDataProvider);
			var repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());
            IProductionManagerWrapper productionManagerWrapper =
                new ProductionManagerWrapper(toggleProvider, repositoryFactory.GetProductionRepository(0),
                    serviceManagerProvider, logger);
			Func<IProductionManager> productionManagerFactory = () => new ProductionManager(logger, repositoryFactory, productionManagerWrapper);

			return new RelativityProviderSourceConfiguration(helper, productionManagerFactory, managerFactory, federatedInstanceManager);
		}
	}
}