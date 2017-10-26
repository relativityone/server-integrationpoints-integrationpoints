using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class RestoreJobHistoryDestinationWorkspaceCommand : ICommand
	{
		private readonly IRSAPIService _rsapiService;
		private readonly JobHistoryDestinationWorkspaceParser _parser;

		public RestoreJobHistoryDestinationWorkspaceCommand(IRSAPIService rsapiService, JobHistoryDestinationWorkspaceParser parser)
		{
			_rsapiService = rsapiService;
			_parser = parser;
		}

		public void Execute()
		{
			Query<RDO> query = new Query<RDO>
			{
				Fields = FieldValue.AllFields,
				Condition = new NotCondition(new TextCondition(new Guid(JobHistoryFieldGuids.DestinationInstance), TextConditionEnum.IsSet))
			};

			List<JobHistory> jobHistories = _rsapiService.JobHistoryLibrary.Query(query);
			foreach (var jobHistory in jobHistories)
			{

				DestinationWorkspaceElementsParsingResult result = _parser.Parse(jobHistory.DestinationWorkspace);
				jobHistory.DestinationWorkspace = result.WorkspaceName;
				jobHistory.DestinationInstance = result.InstanceName;
				_rsapiService.JobHistoryLibrary.Update(jobHistory);
			}
		}

		public string SuccessMessage => "Successfully updated Job History RDOs.";
		public string FailureMessage => "Failed to update Job History RDOs.";
	}
}