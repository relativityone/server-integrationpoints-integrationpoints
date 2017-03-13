﻿using System;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class RestoreJobHistoryDestinationWorkspaceCommand : ICommand
	{
		private readonly IRSAPIService _rsapiService;
		private readonly IDestinationParser _destinationParser;
		private readonly IFederatedInstanceManager _federatedInstanceManager;

		public RestoreJobHistoryDestinationWorkspaceCommand(IRSAPIService rsapiService, IDestinationParser destinationParser, IFederatedInstanceManager federatedInstanceManager)
		{
			_rsapiService = rsapiService;
			_destinationParser = destinationParser;
			_federatedInstanceManager = federatedInstanceManager;
		}

		public void Execute()
		{
			Query<RDO> query = new Query<RDO>
			{
				Fields = FieldValue.AllFields,
				Condition = new NotCondition(new TextCondition(new Guid(JobHistoryFieldGuids.DestinationInstance), TextConditionEnum.IsSet))
			};
			var jobHistories = _rsapiService.JobHistoryLibrary.Query(query);
			foreach (var jobHistory in jobHistories)
			{
				var elements = _destinationParser.GetElements(jobHistory.DestinationWorkspace);
				string instanceName = GetInstanceName(elements);
				int? instanceArtifactId = GetInstanceArtifactId(instanceName);
				string workspaceName = GetWorkspaceName(elements);
				int workspaceArtifactId = GetWorkspaceArtifactId(elements);

				jobHistory.DestinationWorkspace = Utils.GetFormatForWorkspaceOrJobDisplay(workspaceName, workspaceArtifactId);
				jobHistory.DestinationInstance = Utils.GetFormatForWorkspaceOrJobDisplay(instanceName, instanceArtifactId);
				_rsapiService.JobHistoryLibrary.Update(jobHistory);
			}
		}

		private string GetInstanceName(string[] elements)
		{
			if (elements.Length == 3)
			{
				return elements[0].Trim();
			}
			return FederatedInstanceManager.LocalInstance.Name;
		}

		private string GetWorkspaceName(string[] elements)
		{
			if (elements.Length == 3)
			{
				return elements[1].Trim();
			}
			return elements[0].Trim();
		}

		private int GetWorkspaceArtifactId(string[] elements)
		{
			return int.Parse(elements[elements.Length - 1]);
		}

		private int? GetInstanceArtifactId(string instanceName)
		{
			if (instanceName == FederatedInstanceManager.LocalInstance.Name)
			{
				return null;
			}
			var federatedInstance = _federatedInstanceManager.RetrieveFederatedInstanceByName(instanceName);
			return federatedInstance?.ArtifactId;
		}

		public string SuccessMessage => "Successfully updated Job History RDOs.";
		public string FailureMessage => "Failed to update Job History RDOs.";
	}
}