using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;
using Relativity.Toggles;
using Relativity.Toggles.Providers;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories
{
	public class RelativityProviderSourceConfigurationFactory
	{
		public static IRelativityProviderConfiguration Create(IEHHelper helper, IFederatedInstanceModelFactory federatedInstanceModelFactory)
		{
			IToggleProvider toggleProvider = new AlwaysDisabledToggleProvider();
			IManagerFactory managerFactory = new ManagerFactory(helper, toggleProvider);
			IContextContainerFactory contextContainerFactory = new ContextContainerFactory();
			ITokenProvider tokenProvider = new RelativityCoreTokenProvider();
			ISerializer serializer = new JSONSerializer();
			IHelperFactory helperFactory = new HelperFactory(managerFactory, contextContainerFactory, tokenProvider, serializer);

			return new RelativityProviderSourceConfiguration(helper, helperFactory, managerFactory, contextContainerFactory, federatedInstanceModelFactory);
		}
	}
}