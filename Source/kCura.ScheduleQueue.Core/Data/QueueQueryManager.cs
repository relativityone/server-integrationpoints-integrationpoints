using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Data
{
    public class QueueQueryManager : IQueueQueryManager
    {
        private readonly IQueueDBContext _queueDbContext;

        public QueueQueryManager(IHelper helper, Guid agentGuid)
        {
            string queueTable = $"ScheduleAgentQueue_{agentGuid.ToString().ToUpperInvariant()}";

            _queueDbContext = new QueueDBContext(helper, queueTable);
        }

        public ICommand CreateScheduleQueueTable()
        {
            return new CreateScheduleQueueTable(_queueDbContext);
        }

        public ICommand AddCustomColumnsToQueueTable()
        {
            return new AddCustomColumnsToQueueTable(_queueDbContext);
        }

        public IQuery<DataRow> GetAgentTypeInformation(Guid agentGuid)
        {
            return new GetAgentTypeInformation(_queueDbContext.EddsDBContext, agentGuid);
        }

        public IQuery<DataTable> GetNextJob(int agentId, int agentTypeId, int[] resourceGroupArtifactId)
        {
            return new GetNextJob(_queueDbContext, agentId, agentTypeId, resourceGroupArtifactId);
        }

        public IQuery<DataTable> GetNextJob(int agentId, int agentTypeId, long? rootJobId)
        {
            return new GetNextJobWithoutResourceGroup(_queueDbContext, agentId, agentTypeId, rootJobId);
        }

        public ICommand UnlockScheduledJob(int agentId)
        {
            return new UnlockScheduledJob(_queueDbContext, agentId);
        }

        public ICommand UnlockJob(long jobId)
        {
            return new UnlockJob(_queueDbContext, jobId);
        }

        public ICommand DeleteJob(long jobId)
        {
            return new DeleteJob(_queueDbContext, jobId);
        }

        public IQuery<DataTable> CreateScheduledJob(
            int workspaceID,
            int relatedObjectArtifactID,
            string taskType,
            DateTime nextRunTime,
            int AgentTypeID,
            string scheduleRuleType,
            string serializedScheduleRule,
            string jobDetails,
            int jobFlags,
            int SubmittedBy,
            long? rootJobID,
            long? parentJobID = null)
        {
            return new CreateScheduledJob(
                _queueDbContext,
                workspaceID,
                relatedObjectArtifactID,
                taskType,
                nextRunTime,
                AgentTypeID,
                scheduleRuleType,
                serializedScheduleRule,
                jobDetails,
                jobFlags,
                SubmittedBy,
                rootJobID,
                parentJobID);
        }

        public ICommand CreateNewAndDeleteOldScheduledJob(
            long oldScheduledJobId,
            int workspaceID,
            int relatedObjectArtifactID,
            string taskType,
            DateTime nextRunTime,
            int AgentTypeID,
            string scheduleRuleType,
            string serializedScheduleRule,
            string jobDetails,
            int jobFlags,
            int SubmittedBy,
            long? rootJobID,
            long? parentJobID = null)
        {
            return new CreateNewAndDeleteOldScheduledJob(
                _queueDbContext,
                oldScheduledJobId,
                workspaceID,
                relatedObjectArtifactID,
                taskType,
                nextRunTime,
                AgentTypeID,
                scheduleRuleType,
                serializedScheduleRule,
                jobDetails,
                jobFlags,
                SubmittedBy,
                rootJobID,
                parentJobID);
        }

        public ICommand CleanupJobQueueTable()
        {
            return new CleanupJobQueueTable(_queueDbContext);
        }

        public ICommand CleanupScheduledJobsQueue()
        {
            return new CleanupScheduledJobsQueue(_queueDbContext);
        }

        public IQuery<DataTable> GetAllJobs()
        {
            return new GetAllJobs(_queueDbContext);
        }

        public IQuery<int> UpdateStopState(IList<long> jobIds, StopState state)
        {
            return new UpdateStopState(_queueDbContext, jobIds, state);
        }

        public IQuery<DataTable> GetJobByRelatedObjectIdAndTaskType(int workspaceId, int relatedObjectArtifactId, List<string> taskTypes)
        {
            return new GetJobByRelatedObjectIdAndTaskType(_queueDbContext, workspaceId, relatedObjectArtifactId, taskTypes);
        }

        public IQuery<DataTable> GetJobsByIntegrationPointId(long integrationPointId)
        {
            return new GetJobsByIntegrationPointId(_queueDbContext, integrationPointId);
        }

        public IQuery<DataTable> GetJob(long jobId)
        {
            return new GetJob(_queueDbContext, jobId);
        }

        public ICommand UpdateJobDetails(long jobId, string jobDetails)
        {
            return new UpdateJobDetails(_queueDbContext, jobId, jobDetails);
        }

        public IQuery<bool> CheckAllSyncWorkerBatchesAreFinished(long rootJobId)
        {
            return new CheckAllSyncWorkerBatchesAreFinished(_queueDbContext, rootJobId);
        }

        public IQuery<int> Heartbeat(long jobId, DateTime heartbeatTime)
        {
            return new UpdateHeartbeat(_queueDbContext, jobId, heartbeatTime);
        }
    }
}
