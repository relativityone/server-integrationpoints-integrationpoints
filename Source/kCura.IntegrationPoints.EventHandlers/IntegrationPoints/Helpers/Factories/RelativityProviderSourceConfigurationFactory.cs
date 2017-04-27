using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories
{
	public class RelativityProviderSourceConfigurationFactory
	{
		public static IRelativityProviderConfiguration Create(IEHHelper helper, IFederatedInstanceModelFactory federatedInstanceModelFactory, IInstanceSettingsManager federatedInstanceManager)
		{
			Apps.Common.Config.Manager.Settings.Factory = new HelperConfigSqlServiceFactory(helper);

			IConfigFactory configFactory = new ConfigFactory();
			ICredentialProvider credentialProvider = new TokenCredentialProvider();
			ISerializer serializer = new JSONSerializer();
			ITokenProvider tokenProvider = new RelativityCoreTokenProvider();
			IServiceManagerProvider serviceManagerProvider = new ServiceManagerProvider(configFactory, credentialProvider,serializer, tokenProvider);

			IManagerFactory managerFactory = new ManagerFactory(helper, serviceManagerProvider);
			IContextContainerFactory contextContainerFactory = new ContextContainerFactory();
			IIntegrationPointSerializer integrationPointSerializer = new IntegrationPointSerializer();
			IHelperFactory helperFactory = new HelperFactory(managerFactory, contextContainerFactory, tokenProvider, integrationPointSerializer);
			
			return new RelativityProviderSourceConfiguration(helper, helperFactory, managerFactory, contextContainerFactory, federatedInstanceModelFactory, federatedInstanceManager);
		}
	}
}