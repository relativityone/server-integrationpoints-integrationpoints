using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Domain.Managers;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Factories
{
	public class RestoreJobHistoryDestinationWorkspaceCommandFactory
	{
		public static ICommand Create(IHelper helper, int workspaceArtifactId)
		{
			IRSAPIService rsapiService = new RSAPIService(helper, workspaceArtifactId);
			IDestinationParser destinationParser = new DestinationParser();
			IFederatedInstanceManager federatedInstanceManager = new FederatedInstanceManager(new RepositoryFactory(helper, helper.GetServicesManager()));
			return new RestoreJobHistoryDestinationWorkspaceCommand(rsapiService, destinationParser, federatedInstanceManager);
		}
	}
}