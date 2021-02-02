using System;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Authentication.WebApi;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
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
			IConfigFactory configFactory = new ConfigFactory();
			IAPILog logger = helper.GetLoggerFactory().GetLogger();
			IWebApiLoginService credentialProvider = WebApiLoginServiceFactoryDeprecated.Create(logger);
			ISqlServiceFactory sqlServiceFactory = new HelperConfigSqlServiceFactory(helper);
			IServiceManagerProvider serviceManagerProvider = new ServiceManagerProvider(configFactory, credentialProvider, sqlServiceFactory);

			IManagerFactory managerFactory = new ManagerFactory(helper, new EmptyRemovableAgent());
			var repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());
			Func<IProductionManager> productionManagerFactory = () => new ProductionManager(logger, repositoryFactory, serviceManagerProvider);

			return new RelativityProviderSourceConfiguration(helper, productionManagerFactory, managerFactory, federatedInstanceManager);
		}
	}
}