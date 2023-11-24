﻿using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Properties;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Core.Services
{
    public class AgentJobManager : IJobManager
    {
        private readonly IEddsServiceContext _context;
        private readonly IJobService _jobService;
        private readonly ILogger<AgentJobManager> _logger;
        private readonly ISerializer _serializer;
        private readonly IJobTracker _tracker;

        public AgentJobManager(IEddsServiceContext context, IJobService jobService, ISerializer serializer, IJobTracker tracker, ILogger<AgentJobManager> logger)
        {
            _context = context;
            _jobService = jobService;
            _serializer = serializer;
            _tracker = tracker;
            _logger = logger;
        }

        public void CreateJob(TaskParameters jobDetails, TaskType task, string correlationId, int workspaceId, int integrationPointId, IScheduleRule rule, long? rootJobID = null, long? parentJobID = null)
        {
            try
            {
                string serializedDetails = null;
                if (jobDetails != null)
                {
                    serializedDetails = _serializer.Serialize(jobDetails);
                }
                if (rule != null)
                {
                    _jobService.CreateJob(workspaceId, integrationPointId, correlationId, task.ToString(), rule, serializedDetails, _context.UserID, rootJobID, parentJobID);
                }
                else
                {
                    _jobService.CreateJob(workspaceId, integrationPointId, correlationId, task.ToString(), DateTime.UtcNow, serializedDetails, _context.UserID, rootJobID, parentJobID);
                }
            }
            catch (AgentNotFoundException anfe)
            {
                LogCreatingJobError(anfe, task, workspaceId, integrationPointId);
                throw new Exception(ErrorMessages.NoAgentInstalled, anfe);
            }
        }

        public Job CreateJob(Job parentJob, TaskParameters jobDetails, TaskType task)
        {
            return CreateJob(jobDetails, task, parentJob.CorrelationID, parentJob.WorkspaceID, parentJob.RelatedObjectArtifactID, GetRootJobId(parentJob), parentJob.JobId);
        }

        public Job CreateJobWithTracker(Job parentJob, TaskParameters jobDetails, TaskType type, string batchId)
        {
            Job job = CreateJobInternal(jobDetails, type, parentJob.CorrelationID, parentJob.WorkspaceID, parentJob.RelatedObjectArtifactID, parentJob.SubmittedBy, GetRootJobId(parentJob), parentJob.JobId);
            _tracker.CreateTrackingEntry(job, batchId);

            return job;
        }

        public bool CheckBatchOnJobComplete(Job job, string batchId, bool isBatchFinished = true)
        {
            return _tracker.CheckEntries(job, batchId, isBatchFinished);
        }

        public BatchStatusQueryResult GetBatchesStatuses(Job job, string batchId)
        {
            return _tracker.GetBatchesStatuses(job, batchId);
        }

        public Job CreateJob(TaskParameters jobDetails, TaskType task, string correlationId, int workspaceId, int integrationPointId,
            long? rootJobId = null, long? parentJobId = null)
        {
            return CreateJobInternal(jobDetails, task, correlationId, workspaceId, integrationPointId, _context.UserID, rootJobId, parentJobId);
        }

        public Job CreateJobOnBehalfOfAUser(TaskParameters jobDetails, TaskType task, string correlationId, int workspaceId, int integrationPointId, int userId, long? rootJobId = null,
            long? parentJobId = null)
        {
            return CreateJobInternal(jobDetails, task, correlationId, workspaceId, integrationPointId, userId, rootJobId, parentJobId);
        }

        public Job GetJob(int workspaceID, int relatedObjectArtifactID, string taskName)
        {
            return _jobService.GetScheduledJobs(workspaceID, relatedObjectArtifactID, taskName);
        }

        public void DeleteJob(long jobID)
        {
            try
            {
                _jobService.DeleteJob(jobID);
            }
            catch (AgentNotFoundException anfe)
            {
                LogDeletingJobError(jobID, anfe);
                throw new Exception(ErrorMessages.NoAgentInstalled, anfe);
            }
        }

        public IDictionary<Guid, List<Job>> GetJobsByBatchInstanceId(long integrationPointId)
        {
            IDictionary<Guid, List<Job>> results = new Dictionary<Guid, List<Job>>();
            IList<Job> jobs = _jobService.GetJobs(integrationPointId);

            if (jobs == null)
            {
                return results;
            }

            foreach (var job in jobs)
            {
                try
                {
                    TryAddJobToResultsWithTaskParameters(job, results);
                }
                catch (Exception e)
                {
                    // in case of the serialization fails for whatever reasons.
                    LogTaskParametersDeserializationError(e);
                }
            }
            return results;
        }

        public IList<Job> GetJobsByBatchInstanceId(long integrationPointId, Guid batchId)
        {
            IDictionary<Guid, List<Job>> bacthedAgentJobs = GetJobsByBatchInstanceId(integrationPointId);
            if (!bacthedAgentJobs.ContainsKey(batchId))
            {
                LogFailedToFindBatchInstance(integrationPointId, batchId);
                throw new Exception("Unable to find the batch instance id in the scheduled agent queue.");
            }
            return bacthedAgentJobs[batchId];
        }

        public IDictionary<Guid, List<Job>> GetJobsByJobHistoryGuid(long integrationPointId)
        {
            IDictionary<Guid, List<Job>> results = new Dictionary<Guid, List<Job>>();
            IList<Job> jobs = _jobService.GetJobs(integrationPointId);

            if (jobs == null)
            {
                return results;
            }

            const string jobHistoryGuidName = "JobHistoryGuid";

            foreach (var job in jobs)
            {
                try
                {
                    Dictionary<string, object> jobDetails = _serializer.Deserialize<Dictionary<string, object>>(job.JobDetails);

                    bool isTaskParametersParsed = TryAddJobToResultsWithTaskParameters(job, results);

                    if (isTaskParametersParsed)
                    {
                        continue;
                    }

                    bool isJobHistoryGuidParsed = Guid.TryParse(jobDetails[jobHistoryGuidName]?.ToString(), out Guid jobHistoryGuid);
                    if (results.ContainsKey(jobHistoryGuid))
                    {
                        results[jobHistoryGuid].Add(job);
                        continue;
                    }

                    if (isJobHistoryGuidParsed)
                    {
                        results[jobHistoryGuid] = new List<Job> { job };
                    }
                }
                catch (Exception e)
                {
                    // in case of the serialization fails for whatever reasons.
                    LogTaskParametersDeserializationError(e);
                }
            }
            return results;
        }

        public void StopJobs(IList<long> jobIds)
        {
            _jobService.UpdateStopState(jobIds, StopState.Stopping);
        }

        public void CreateJob(int workspaceID, int integrationPointID, string correlationId, TaskType task, string serializedDetails, long? rootJobId = null, long? parentJobId = null)
        {
            try
            {
                _jobService.CreateJob(workspaceID, integrationPointID, correlationId, task.ToString(), DateTime.UtcNow, serializedDetails, _context.UserID, rootJobId, parentJobId);
            }
            catch (AgentNotFoundException anfe)
            {
                LogCreatingJobError(anfe, task, workspaceID, integrationPointID);
                throw new Exception(ErrorMessages.NoAgentInstalled, anfe);
            }
        }

        public static long? GetRootJobId(Job parentJob)
        {
            long? rootJobId = parentJob.RootJobId;

            if (!rootJobId.HasValue)
            {
                rootJobId = parentJob.JobId;
            }

            return rootJobId;
        }

        private Job CreateJobInternal(TaskParameters jobDetails, TaskType task, string correlationId, int workspaceId, int integrationPointId, int userId, long? rootJobId = null, long? parentJobID = null)
        {
            try
            {
                string serializedDetails = null;
                if (jobDetails != null)
                {
                    serializedDetails = _serializer.Serialize(jobDetails);
                }
                return _jobService.CreateJob(workspaceId, integrationPointId, correlationId, task.ToString(), DateTime.UtcNow, serializedDetails, userId, rootJobId, parentJobID);
            }
            catch (AgentNotFoundException anfe)
            {
                LogCreatingJobError(anfe, task, workspaceId, integrationPointId);
                throw new Exception(ErrorMessages.NoAgentInstalled, anfe);
            }
        }

        private bool TryAddJobToResultsWithTaskParameters(Job job, IDictionary<Guid, List<Job>> results)
        {
            TaskParameters parameter = _serializer.Deserialize<TaskParameters>(job.JobDetails);
            if (parameter.BatchInstance == Guid.Empty)
            {
                return false;
            }
            if (results.ContainsKey(parameter.BatchInstance))
            {
                results[parameter.BatchInstance].Add(job);
            }
            else
            {
                results[parameter.BatchInstance] = new List<Job> { job };
            }

            return true;
        }

        #region Logging

        private void LogCreatingJobError(AgentNotFoundException anfe, TaskType task, int workspaceId, int integrationPointId)
        {
            _logger.LogError(anfe, "Failed to create job of type {TaskType} for Integration Point {IntegrationPointId} in Workspace {WorkspaceId}.", task, integrationPointId,
                workspaceId);
        }

        private void LogDeletingJobError(long jobID, AgentNotFoundException anfe)
        {
            _logger.LogError(anfe, "Failed to delete job {JobId}.", jobID);
        }

        private void LogTaskParametersDeserializationError(Exception e)
        {
            _logger.LogError(e, "Failed to deserialize TaskParameters.");
        }

        private void LogFailedToFindBatchInstance(long integrationPointId, Guid batchId)
        {
            _logger.LogError("Unable to find the batch instance id {BatchId} in the scheduled agent queue for Integration Point {IPId}.", batchId.ToString(), integrationPointId);
        }

        #endregion
    }
}
