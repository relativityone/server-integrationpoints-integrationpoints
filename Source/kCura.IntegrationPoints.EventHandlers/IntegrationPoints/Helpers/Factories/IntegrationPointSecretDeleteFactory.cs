using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories
{
    public static class IntegrationPointSecretDeleteFactory
    {
        public static IIntegrationPointSecretDelete Create(IEHHelper helper)
        {
            IAPILog logger = helper.GetLoggerFactory().GetLogger();
            ISecretsRepository secretsRepository = new SecretsRepository(
                SecretStoreFacadeFactory_Deprecated.Create(helper.GetSecretStore, logger),
                logger
            );
            IRelativityObjectManager relativityObjectManager = CreateObjectManager(helper);
            IIntegrationPointRepository integrationPointRepository =
                CreateIntegrationPointRepository(
                    relativityObjectManager,
                    secretsRepository,
                    logger);
            return new IntegrationPointSecretDelete(
                helper.GetActiveCaseID(),
                secretsRepository,
                integrationPointRepository);
        }

        private static IRelativityObjectManager CreateObjectManager(IEHHelper helper)
        {
            return new RelativityObjectManagerFactory(helper).CreateRelativityObjectManager(helper.GetActiveCaseID());
        }

        private static IIntegrationPointRepository CreateIntegrationPointRepository(
            IRelativityObjectManager objectManager,
            ISecretsRepository secretsRepository,
            IAPILog logger)
        {
            return new IntegrationPointRepository(
                objectManager,
                secretsRepository,
                logger
            );
        }
    }
}
