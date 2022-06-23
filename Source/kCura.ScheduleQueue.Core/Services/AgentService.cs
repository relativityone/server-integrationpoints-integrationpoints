using System;
using System.Data;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Services
{
    public class AgentService : IAgentService
    {
        private bool _creationOfQTableHasRun;
        private AgentTypeInformation _agentTypeInformation;

        private readonly IQueueQueryManager _queryManager;
        private readonly IAPILog _logger;

        public AgentService(IHelper dbHelper, IQueueQueryManager queryManager, Guid agentGuid)
        {
            _queryManager = queryManager;

            AgentGuid = agentGuid;
            _logger = dbHelper.GetLoggerFactory().GetLogger().ForContext<AgentService>();
        }

        public Guid AgentGuid { get; }

        public AgentTypeInformation AgentTypeInformation => _agentTypeInformation ?? (_agentTypeInformation = GetAgentTypeInformation());

        public void InstallQueueTable()
        {
            _queryManager.CreateScheduleQueueTable()
                .Execute();

            _queryManager.AddStopStateColumnToQueueTable()
                .Execute();

            _queryManager.AddHeartbeatColumnToQueueTable()
                .Execute();
        }

        public void CreateQueueTableOnce()
        {
            if (!_creationOfQTableHasRun)
            {
                InstallQueueTable();
            }
            _creationOfQTableHasRun = true;
        }

        private AgentTypeInformation GetAgentTypeInformation()
        {
            DataRow row = _queryManager
                .GetAgentTypeInformation(AgentGuid)
                .Execute();

            if (row == null)
            {
                string message = $"The agent with Guid {AgentGuid} could not be found, please ensure there is an existing installed agent";
                throw new AgentNotFoundException(message);
            }

            var agentTypeInformation = new AgentTypeInformation(row);
            return agentTypeInformation;
        }
    }
}