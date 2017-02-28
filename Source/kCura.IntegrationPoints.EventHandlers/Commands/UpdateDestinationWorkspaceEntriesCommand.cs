using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

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

		public string SuccessMessage => "Destination Workspace entries successfully updated.";
		public string FailureMessage => "Failed to update Destination Workspace entries.";

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
			var condition = new NotCondition(new TextCondition(new Guid(DestinationWorkspaceFieldGuids.DestinationInstanceName), TextConditionEnum.IsSet));
			var query = new Query<RDO>
			{
				Fields = FieldValue.AllFields,
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.DestinationWorkspace),
				Condition = condition
			};

			return _rsapiService.DestinationWorkspaceLibrary.Query(query);
		}
	}
}