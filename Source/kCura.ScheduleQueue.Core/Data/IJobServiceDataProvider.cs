using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Data;

namespace kCura.ScheduleQueue.Core.Data
{
    public interface IJobServiceDataProvider
    {
        DataTable GetJobsByIntegrationPointId(long integrationPointId);

        DataRow GetNextQueueJob(int agentId, int agentTypeId, int[] resurceGroupIdsArray);

        DataRow GetNextQueueJob(int agentId, int agentTypeId, long? rootJobId = null);

        void CreateNewAndDeleteOldScheduledJob(long oldScheduledJobId, int workspaceID, int relatedObjectArtifactID,
            Guid correlationID, string taskType, DateTime nextRunTime, int agentTypeId, string scheduleRuleType,
            string serializedScheduleRule, string jobDetails, int jobFlags, int submittedBy, long? rootJobID, long? parentJobID);

        DataRow CreateScheduledJob(int workspaceID, int relatedObjectArtifactID, Guid correlationID, string taskType,
            DateTime nextRunTime, int agentTypeId, string scheduleRuleType, string serializedScheduleRule,
            string jobDetails, int jobFlags, int submittedBy, long? rootJobID, long? parentJobID);

        void DeleteJob(long jobId);

        DataRow GetJob(long jobId);

        DataTable GetJobs(int workspaceId, int relatedObjectArtifactId, string taskType);

        DataTable GetJobs(int workspaceId, int relatedObjectArtifactId, List<string> taskTypes);

        DataTable GetAllJobs();

        int UpdateStopState(IList<long> jobIds, StopState state);

        /// <summary>
        /// Clears the LockedByAgentID value.
        /// </summary>
        void UnlockJob(long jobID, StopState state);

        void UpdateJobDetails(long jobId, string jobDetails);
    }
}
