using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;
using Relativity.Services.Security;
using Relativity.Toggles;
using Relativity.Toggles.Providers;
using IFederatedInstanceManager = kCura.IntegrationPoints.Domain.Managers.IFederatedInstanceManager;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories
{
	public class RelativityProviderSourceConfigurationFactory
	{
		public static IRelativityProviderConfiguration Create(IEHHelper helper, IFederatedInstanceModelFactory federatedInstanceModelFactory, IInstanceSettingsManager federatedInstanceManager)
		{
			IToggleProvider toggleProvider = new AlwaysDisabledToggleProvider();
			IManagerFactory managerFactory = new ManagerFactory(helper, toggleProvider);
			IContextContainerFactory contextContainerFactory = new ContextContainerFactory();
			ITokenProvider tokenProvider = new RelativityCoreTokenProvider();
			IIntegrationPointSerializer serializer = new IntegrationPointSerializer();
			IHelperFactory helperFactory = new HelperFactory(managerFactory, contextContainerFactory, tokenProvider, serializer);
			

			return new RelativityProviderSourceConfiguration(helper, helperFactory, managerFactory, contextContainerFactory, federatedInstanceModelFactory, federatedInstanceManager);
		}
	}
}