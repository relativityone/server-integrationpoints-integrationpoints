using kCura.IntegrationPoints.Contracts.RDO;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Data.Adaptors.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories
{
	public class RelativityProviderSourceConfigurationFactory
	{
		public static IRelativityProviderConfiguration Create(IEHHelper helper)
		{
			IManagerFactory managerFactory = new ManagerFactory(helper);
			IContextContainerFactory contextContainerFactory = new ContextContainerFactory();
			ITokenProvider tokenProvider = new RelativityCoreTokenProvider();
			IHelperFactory helperFactory = new HelperFactory(managerFactory, contextContainerFactory, tokenProvider);

			IObjectQueryManagerAdaptor objectQueryManagerAdaptor = new ObjectQueryManagerAdaptor(helper,
				helper.GetServicesManager(), -1, (int) ArtifactType.Case);
			
			return new RelativityProviderSourceConfiguration(helper, helperFactory, managerFactory, contextContainerFactory);
		}
	}
}