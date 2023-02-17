using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Facades.ObjectManager;
using kCura.IntegrationPoints.Data.Facades.ObjectManager.Implementation;
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
            IRepositoryFactory repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());
            Domain.Managers.IFederatedInstanceManager federatedInstanceManager = new FederatedInstanceManager();
            Domain.Managers.IInstanceSettingsManager instanceSettingsManager =new InstanceSettingsManager(repositoryFactory);
            IRelativityProviderConfiguration relativityProviderSourceConfiguration =
                RelativityProviderSourceConfigurationFactory.Create(helper, instanceSettingsManager);

            IRelativityProviderConfiguration relativityProviderDestinationConfiguration =
                new RelativityProviderDestinationConfiguration(helper, federatedInstanceManager, repositoryFactory);

            IAPILog logger = helper.GetLoggerFactory().GetLogger();
            int workspaceId = helper.GetActiveCaseID();
            IRetryHandlerFactory retryHandlerFactory = new RetryHandlerFactory(logger);
            IExternalServiceInstrumentationProvider instrumentationProvider =
                new ExternalServiceInstrumentationProviderWithoutJobContext(logger);

            IObjectManagerFacadeFactory objectManagerFacadeFactory = new ObjectManagerFacadeFactory(
                helper.GetServicesManager(),
                logger,
                instrumentationProvider,
                retryHandlerFactory);

            IRelativityObjectManager relativityObjectManager = new RelativityObjectManager(
                workspaceId,
                logger,
                objectManagerFacadeFactory);

            return new IntegrationPointViewPreLoad(
                caseServiceContext,
                relativityProviderSourceConfiguration,
                relativityProviderDestinationConfiguration,
                integrationPointBaseFieldsConstants,
                relativityObjectManager,
                helper);
        }
    }
}
