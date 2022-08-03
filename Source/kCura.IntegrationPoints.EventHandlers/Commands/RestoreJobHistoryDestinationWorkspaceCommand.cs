using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.Commands.RestoreJobHistoryParser;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class RestoreJobHistoryDestinationWorkspaceCommand : ICommand
    {
        private readonly IRelativityObjectManager _objectManager;
        private readonly JobHistoryDestinationWorkspaceParser _parser;

        public RestoreJobHistoryDestinationWorkspaceCommand(IRelativityObjectManager objectManager, JobHistoryDestinationWorkspaceParser parser)
        {
            _objectManager = objectManager;
            _parser = parser;
        }

        public void Execute()
        {
            var request = new QueryRequest
            {
                Condition = $"NOT '{JobHistoryFields.DestinationInstance}' ISSET"
            };
            List<JobHistory> jobHistories = _objectManager.Query<JobHistory>(request);
            foreach (JobHistory jobHistory in jobHistories)
            {

                DestinationWorkspaceElementsParsingResult result = _parser.Parse(jobHistory.DestinationWorkspace);
                jobHistory.DestinationWorkspace = result.WorkspaceName;
                jobHistory.DestinationInstance = result.InstanceName;
                _objectManager.Update(jobHistory);
            }
        }

        public string SuccessMessage => "Successfully updated Job History RDOs.";
        public string FailureMessage => "Failed to update Job History RDOs.";
    }
}