using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class UpdateDestinationWorkspaceEntriesCommand : ICommand
	{
		private readonly IRSAPIService _rsapiService;
		private readonly IDestinationWorkspaceRepository _destinationWorkspaceRepository;

		public UpdateDestinationWorkspaceEntriesCommand(IRSAPIService rsapiService, IDestinationWorkspaceRepository destinationWorkspaceRepository)
		{
			_rsapiService = rsapiService;
			_destinationWorkspaceRepository = destinationWorkspaceRepository;
		}

		public void Execute()
		{
			var thisInstance = FederatedInstanceManager.LocalInstance;
			var entriesToUpdate = GetDestinationWorkspacesToUpdate();
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

			return _rsapiService.RelativityObjectManager.Query<DestinationWorkspace>(query);
		}
	}
}