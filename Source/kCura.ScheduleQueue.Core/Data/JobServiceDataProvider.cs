using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Data;

namespace kCura.ScheduleQueue.Core.Data
{
    public class JobServiceDataProvider : IJobServiceDataProvider
    {
        private readonly IQueueQueryManager _queueManager;

        public JobServiceDataProvider(IQueueQueryManager queueManager)
        {
            _queueManager = queueManager;
        }

        public DataRow GetNextQueueJob(int agentId, int agentTypeId, int[] resourceGroupIdsArray)
        {
            using (DataTable dataTable = _queueManager.GetNextJob(agentId, agentTypeId, resourceGroupIdsArray).Execute())
            {
                return GetFirstRowOrDefault(dataTable);
            }
        }

        public DataRow GetNextQueueJob(int agentId, int agentTypeId, long? rootJobId = null)
        {
            using (DataTable dataTable = _queueManager.GetNextJob(agentId, agentTypeId, rootJobId).Execute())
            {
                return GetFirstRowOrDefault(dataTable);
            }
        }

        private DataRow GetFirstRowOrDefault(DataTable dataTable)
        {
            return dataTable?.Rows?.Count > 0 ? dataTable.Rows[0] : null;
        }

        public void UnlockJob(long jobID, StopState state)
        {
            _queueManager
                .UnlockJob(jobID, state)
                .Execute();
        }

        public void UpdateJobDetails(long jobId, string jobDetails)
        {
            _queueManager
                .UpdateJobDetails(jobId, jobDetails)
                .Execute();
        }

        public void CreateNewAndDeleteOldScheduledJob(long oldScheduledJobId, int workspaceID, int relatedObjectArtifactID, string taskType,
            DateTime nextRunTime, int agentTypeId, string scheduleRuleType, string serializedScheduleRule,
            string jobDetails, int jobFlags, int submittedBy, long? rootJobID, long? parentJobID)
        {
            _queueManager.CreateNewAndDeleteOldScheduledJob(oldScheduledJobId, workspaceID, relatedObjectArtifactID,
                    taskType, nextRunTime, agentTypeId, scheduleRuleType, serializedScheduleRule, jobDetails,
                    jobFlags, submittedBy, rootJobID, parentJobID)
                .Execute();
        }

        public DataRow CreateScheduledJob(int workspaceID, int relatedObjectArtifactID, string taskType,
            DateTime nextRunTime, int agentTypeId, string scheduleRuleType, string serializedScheduleRule,
            string jobDetails, int jobFlags, int submittedBy, long? rootJobID, long? parentJobID)
        {
            using (DataTable dataTable = _queueManager.CreateScheduledJob(workspaceID, relatedObjectArtifactID,
                    taskType, nextRunTime, agentTypeId, scheduleRuleType, serializedScheduleRule, jobDetails,
                    jobFlags, submittedBy, rootJobID, parentJobID)
                .Execute())
            {
                return GetFirstRowOrDefault(dataTable);
            }
        }

        public DataTable GetJobsByIntegrationPointId(long integrationPointId)
        {
            return _queueManager
                .GetJobsByIntegrationPointId(integrationPointId)
                .Execute();
        }

        public void DeleteJob(long jobId)
        {
            _queueManager
                .DeleteJob(jobId)
                .Execute();
        }

        public DataRow GetJob(long jobId)
        {
            using (DataTable dataTable = _queueManager.GetJob(jobId).Execute())
            {
                return GetFirstRowOrDefault(dataTable);
            }
        }

        public DataTable GetJobs(int workspaceId, int relatedObjectArtifactId, string taskType)
        {
            return GetJobs(workspaceId, relatedObjectArtifactId, new List<string> { taskType });
        }

        public DataTable GetJobs(int workspaceId, int relatedObjectArtifactId, List<string> taskTypes)
        {
            return _queueManager
                .GetJobByRelatedObjectIdAndTaskType(workspaceId, relatedObjectArtifactId, taskTypes)
                .Execute();
        }

        public DataTable GetAllJobs()
        {
            return _queueManager
                .GetAllJobs()
                .Execute();
        }

        public int UpdateStopState(IList<long> jobIds, StopState state)
        {
            return _queueManager
                .UpdateStopState(jobIds, state)
                .Execute();
        }
    }
}
