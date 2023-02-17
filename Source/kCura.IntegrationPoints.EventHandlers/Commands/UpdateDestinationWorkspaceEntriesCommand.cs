using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class UpdateDestinationWorkspaceEntriesCommand : ICommand
    {
        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly IDestinationWorkspaceRepository _destinationWorkspaceRepository;

        public UpdateDestinationWorkspaceEntriesCommand(IRelativityObjectManager relativityObjectManager, IDestinationWorkspaceRepository destinationWorkspaceRepository)
        {
            _relativityObjectManager = relativityObjectManager;
            _destinationWorkspaceRepository = destinationWorkspaceRepository;
        }

        public void Execute()
        {
            FederatedInstanceDto thisInstance = FederatedInstanceManager.LocalInstance;
            IList<DestinationWorkspace> entriesToUpdate = GetDestinationWorkspacesToUpdate();

            foreach (var destinationWorkspace in entriesToUpdate)
            {
                destinationWorkspace.DestinationInstanceName = thisInstance.Name;
                _destinationWorkspaceRepository.Update(destinationWorkspace);
            }
        }

        private IList<DestinationWorkspace> GetDestinationWorkspacesToUpdate()
        {
            string condition = $"NOT '{DestinationWorkspaceFields.DestinationInstanceName}' ISSET";
            var query = new QueryRequest
            {
                Condition = condition
            };

            return _relativityObjectManager.Query<DestinationWorkspace>(query);
        }
    }
}
