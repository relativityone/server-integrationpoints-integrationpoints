using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public interface IJobService
    {
        AgentTypeInformation AgentTypeInformation { get; }

        Job GetNextQueueJob(IEnumerable<int> resourceGroupIds, int agentID, long? rootJobId = null);

        DateTime? GetJobNextUtcRunDateTime(Job job, IScheduleRuleFactory scheduleRuleFactory, TaskResult taskResult);

        FinalizeJobResult FinalizeJob(Job job, IScheduleRuleFactory scheduleRuleFactory, TaskResult taskResult);

        Job CreateJob(int workspaceID, int relatedObjectArtifactID, string correlationID, string taskType,
            IScheduleRule scheduleRule, string jobDetails, int SubmittedBy, long? rootJobID, long? parentJobID);

        Job CreateJob(int workspaceID, int relatedObjectArtifactID, string correlationID, string taskType,
            DateTime nextRunTime, string jobDetails, int SubmittedBy, long? rootJobID, long? parentJobID);

        void DeleteJob(long jobID);

        Job GetJob(long jobID);

        Job GetScheduledJobs(int workspaceID, int relatedObjectArtifactID, string taskName);

        IEnumerable<Job> GetScheduledJobs(int workspaceID, int relatedObjectArtifactID, List<string> taskTypes);

        IEnumerable<Job> GetAllScheduledJobs();

        void UpdateStopState(IList<long> jobIds, StopState state);

        /// <summary>
        /// Get a list of job RDOs that associate with the integration point object.
        /// </summary>
        /// <param name="integrationPointId">An artifact id of integration point object.</param>
        /// <returns>A list of job DTOs</returns>
        IList<Job> GetJobs(long integrationPointId);

        void UpdateJobDetails(Job job);

        void FinalizeDrainStoppedJob(Job job);

        void UnlockJob(Job job);
    }
}
