using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Factories
{
	public class RestoreJobHistoryDestinationWorkspaceCommandFactory
	{
		public static ICommand Create(IHelper helper, int workspaceArtifactId)
		{
			IRSAPIService rsapiService = new RSAPIService(helper, workspaceArtifactId);
			var repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());
			IFederatedInstanceManager federatedInstanceManager = new FederatedInstanceManager(repositoryFactory);
			IWorkspaceManager workspaceManager = new WorkspaceManager(repositoryFactory);
			var parser = new JobHistoryDestinationWorkspaceParser(workspaceArtifactId, federatedInstanceManager, workspaceManager);
			return new RestoreJobHistoryDestinationWorkspaceCommand(rsapiService, parser);
		}
	}
}