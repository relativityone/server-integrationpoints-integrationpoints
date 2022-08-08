using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Factories
{
    public class UpdateDestinationWorkspaceEntriesFactory
    {
        public static ICommand Create(IHelper helper, int workspaceId)
        {
            IRSAPIService rsapiService = new RSAPIService(helper, workspaceId);
            IDestinationWorkspaceRepository destinationWorkspaceRepository = new DestinationWorkspaceRepository(helper, workspaceId, rsapiService);
            return new UpdateDestinationWorkspaceEntriesCommand(rsapiService, destinationWorkspaceRepository);
        }
    }
}