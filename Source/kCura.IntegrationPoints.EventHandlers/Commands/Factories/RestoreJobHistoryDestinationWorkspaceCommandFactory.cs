using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Factories
{
    public static class RestoreJobHistoryDestinationWorkspaceCommandFactory
    {
        public static ICommand Create(IHelper helper, int workspaceArtifactId)
        {
            IRelativityObjectManager objectManager = CreateObjectManager(helper, workspaceArtifactId);
            var repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());
            IFederatedInstanceManager federatedInstanceManager = new FederatedInstanceManager();
            IWorkspaceManager workspaceManager = new WorkspaceManager(repositoryFactory);
            var parser = new JobHistoryDestinationWorkspaceParser(workspaceArtifactId, federatedInstanceManager, workspaceManager);
            return new RestoreJobHistoryDestinationWorkspaceCommand(objectManager, parser);
        }

        private static IRelativityObjectManager CreateObjectManager(IHelper helper, int workspaceId)
        {
            var factory = new RelativityObjectManagerFactory(helper);
            return factory.CreateRelativityObjectManager(workspaceId);
        }
    }
}