using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories
{
	public class IntegrationPointViewPreLoadFactory
	{
		public static IIntegrationPointViewPreLoad Create(IEHHelper helper, IIntegrationPointBaseFieldsConstants integrationPointBaseFieldsConstants)
		{
			ICaseServiceContext caseServiceContext = ServiceContextFactory.CreateCaseServiceContext(helper, helper.GetActiveCaseID());

			IFederatedInstanceModelFactory federatedInstanceModelFactory;
			if (integrationPointBaseFieldsConstants.Name == IntegrationPointFieldGuids.Name)
			{
				federatedInstanceModelFactory = new IntegrationPointFederatedInstanceModelFactory(caseServiceContext);
			}
			else
			{
				federatedInstanceModelFactory = new IntegrationPointProfileFederatedInstanceModelFactory();
			}


			IRelativityProviderConfiguration relativityProviderSourceConfiguration =
				RelativityProviderSourceConfigurationFactory.Create(helper, federatedInstanceModelFactory);

			IRelativityProviderConfiguration relativityProviderDestinationConfiguration =
				new RelativityProviderDestinationConfiguration(helper);

			return new IntegrationPointViewPreLoad(
				caseServiceContext,
				relativityProviderSourceConfiguration,
				relativityProviderDestinationConfiguration,
				integrationPointBaseFieldsConstants);
		}
	}
}