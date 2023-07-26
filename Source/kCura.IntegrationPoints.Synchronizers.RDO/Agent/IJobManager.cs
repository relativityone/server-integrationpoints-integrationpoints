using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DTO;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public interface IJobManager
    {
        Job CreateJobOnBehalfOfAUser(TaskParameters jobDetails, TaskType task, int workspaceId, int integrationPointId, int userId, long? rootJobId = null, long? parentJobId = null);

        void CreateJob(TaskParameters jobDetails, TaskType task, int workspaceId, int integrationPointId, IScheduleRule rule, long? rootJobID = null, long? parentJobID = null);

        Job CreateJob(TaskParameters jobDetails, TaskType task, int workspaceId, int integrationPointId,
            long? rootJobId = null, long? parentJobId = null);

        Job CreateJob(Job parentJob, TaskParameters jobDetails, TaskType task);

        void DeleteJob(long jobID);

        Job GetJob(int workspaceID, int relatedObjectArtifactID, string taskName);

        Job CreateJobWithTracker(Job parentJob, TaskParameters jobDetails, TaskType type, string batchId);

        bool CheckBatchOnJobComplete(Job job, string batchId, bool isBatchFinished = true);

        BatchStatusQueryResult GetBatchesStatuses(Job job, string batchId);

        /// <summary>
        /// Stops the scheduled queue jobs.
        /// </summary>
        /// <param name="jobIds">A list of scheduled queue job ids to stop.</param>
        void StopJobs(IList<long> jobIds);

        /// <summary>
        /// Gets scheduled agent jobs as a dictionary where the key is the job history's batch instance id and the value is a list of scheduled agent job dtos.
        /// </summary>
        /// <param name="integrationPointId">The artifact id of the Integration Point object.</param>
        /// <returns>A dictionary of batch instance id and its agent job DTOs.</returns>
        IDictionary<Guid, List<Job>> GetJobsByBatchInstanceId(long integrationPointId);

        /// <summary>
        /// Gets scheduled agent jobs by batch instance id.
        /// </summary>
        /// <param name="integrationPointId">The artifact id of the Integration Point object.</param>
        /// <param name="batchId">The batch instance id of the scheduled agent jobs.</param>
        /// <returns>A List of scheduled agent jobs.</returns>
        IList<Job> GetJobsByBatchInstanceId(long integrationPointId, Guid batchId);
    }
}
