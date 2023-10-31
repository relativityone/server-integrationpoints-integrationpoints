using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;
using Relativity.API;
using IKubernetesMode = kCura.IntegrationPoints.Domain.EnvironmentalVariables.IKubernetesMode;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public class JobService : IJobService
    {
        private readonly IKubernetesMode _kubernetesMode;
        private readonly IAPILog _log;
        private readonly ISerializer _serializer;
        private readonly IAgentService _agentService;
        private readonly IJobServiceDataProvider _dataProvider;

        public JobService(IAgentService agentService, IJobServiceDataProvider dataProvider, IKubernetesMode kubernetesMode, IHelper dbHelper)
        {
            _kubernetesMode = kubernetesMode;
            _agentService = agentService;
            _log = dbHelper.GetLoggerFactory().GetLogger().ForContext<JobService>();
            _dataProvider = dataProvider;
            _serializer = new RipJsonSerializer(_log);
        }

        public AgentTypeInformation AgentTypeInformation => _agentService.AgentTypeInformation;

        public Job GetNextQueueJob(IEnumerable<int> resourceGroupIds, int agentID, long? rootJobId = null)
        {
            DataRow row;

            if (_kubernetesMode.IsEnabled())
            {
                row = _dataProvider.GetNextQueueJob(agentID, AgentTypeInformation.AgentTypeID, rootJobId);
            }
            else
            {
                int[] resourceGroupIdsArray = resourceGroupIds?.ToArray() ?? Array.Empty<int>();

                if (resourceGroupIdsArray.Length == 0)
                {
                    throw new ArgumentException($"Did not find any resource group ids for agent with id '{agentID}'." +
                                                        " Please validate EnableKubernetesMode toggle value, current value: False");
                }

                row = _dataProvider.GetNextQueueJob(agentID, AgentTypeInformation.AgentTypeID, resourceGroupIdsArray);
                _log.LogInformation("Retrieved following row: {@row}", row);
            }

            Job job = CreateJob(row);
            if (job != null)
            {
                LogJobInformation(job, agentID);
            }

            return job;
        }

        public FinalizeJobResult FinalizeJob(Job job, IScheduleRuleFactory scheduleRuleFactory, TaskResult taskResult)
        {
            if (scheduleRuleFactory == null)
            {
                throw new ArgumentNullException(nameof(scheduleRuleFactory));
            }

            if (job == null)
            {
                return new FinalizeJobResult { JobState = JobLogState.Finished };
            }

            LogOnFinalizeJob(job.JobId, job.JobDetails, taskResult);

            var result = new FinalizeJobResult();
            DateTime? nextUtcRunDateTime = null;
            try
            {
                nextUtcRunDateTime = GetJobNextUtcRunDateTime(job, scheduleRuleFactory, taskResult);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unable to get next scheduled runtime for job {@job}", job);
            }

            IScheduleRule scheduleRule = scheduleRuleFactory.Deserialize(job);

            bool shouldBreakSchedule = job.JobFailed?.ShouldBreakSchedule ?? false;

            if (!shouldBreakSchedule && nextUtcRunDateTime.HasValue)
            {
                if (taskResult.Status == TaskStatusEnum.Fail)
                {
                    scheduleRule.IncrementConsecutiveFailedScheduledJobsCount();
                }
                else if (taskResult.Status == TaskStatusEnum.Success)
                {
                    scheduleRule.ResetConsecutiveFailedScheduledJobsCount();
                }
                Guid newJobCorrelationID = Guid.NewGuid();

                _log.LogInformation(
                    "Job {jobId} was scheduled with following details: " +
                    "NextRunTime - {nextRunTime} " +
                    "ScheduleRule - {scheduleRule}" +
                    "CorrelationID - {correlationId}",
                    job.JobId,
                    nextUtcRunDateTime,
                    job.ScheduleRule,
                    newJobCorrelationID);

                TaskParameters taskParameters = new TaskParameters
                {
                    BatchInstance = newJobCorrelationID
                };
                string jobDetails = _serializer.Serialize(taskParameters);
                CreateNewAndDeleteOldScheduledJob(
                    job,
                    newJobCorrelationID.ToString(),
                    scheduleRule,
                    jobDetails);
            }
            else
            {
                _log.LogInformation(
                    "Deleting job {jobId} from the queue - ShouldBreakSchedule {shouldBreakSchedule}, IsScheduled {isScheduledJob}",
                    job.JobId,
                    job.JobFailed?.ShouldBreakSchedule,
                    nextUtcRunDateTime.HasValue);

                DeleteJob(job.JobId);
            }

            result.JobState = JobLogState.Deleted;

            return result;
        }

        public DateTime? GetJobNextUtcRunDateTime(Job job, IScheduleRuleFactory scheduleRuleFactory, TaskResult taskResult)
        {
            if (scheduleRuleFactory == null)
            {
                throw new ArgumentNullException(nameof(scheduleRuleFactory));
            }

            if (job == null)
            {
                _log.LogWarning("Job is null. Return NextUtcRunDateTime as null");
                return null;
            }

            IScheduleRule scheduleRule = scheduleRuleFactory.Deserialize(job);
            DateTime? nextUtcRunDateTime = null;
            if (scheduleRule != null)
            {
                nextUtcRunDateTime = scheduleRule.GetNextUtcRunDateTime(job.NextRunTime);
            }

            _log.LogInformation("NextUtcRunDateTime has been calculated for {nextUtcRunDateTime}.", nextUtcRunDateTime);

            return nextUtcRunDateTime;
        }

        public Job CreateJob(
            int workspaceID,
            int relatedObjectArtifactID,
            string correlationID,
            string taskType,
            IScheduleRule scheduleRule,
            string jobDetails,
            int SubmittedBy,
            long? rootJobID,
            long? parentJobID)
        {
            LogOnCreateJob(workspaceID, relatedObjectArtifactID, taskType, SubmittedBy);
            _agentService.CreateQueueTableOnce();

            Job job;
            DateTime? nextRunTime = null;
            try
            {
                nextRunTime = scheduleRule.GetFirstUtcRunDateTime();
            }
            catch (Exception e)
            {
                _log.LogError(e, "Unable to get first scheduled runtime for parent job Id {parentJobID}", parentJobID);
            }
            if (nextRunTime.HasValue)
            {
                DataRow row = _dataProvider.CreateScheduledJob(
                    workspaceID,
                    relatedObjectArtifactID,
                    correlationID,
                    taskType,
                    nextRunTime.Value,
                    AgentTypeInformation.AgentTypeID,
                    scheduleRule.GetType().AssemblyQualifiedName,
                    scheduleRule.ToSerializedString(),
                    jobDetails,
                    0,
                    SubmittedBy,
                    rootJobID,
                    parentJobID);

                job = CreateJob(row);

                LogOnCreatedScheduledJob(job);
            }
            else
            {
                job = GetScheduledJobs(workspaceID, relatedObjectArtifactID, taskType);
                if (job != null)
                {
                    DeleteJob(job.JobId);
                }
            }
            return job;
        }

        public Job CreateJob(
            int workspaceID,
            int relatedObjectArtifactID,
            string correlationId,
            string taskType,
            DateTime nextRunTime,
            string jobDetails,
            int SubmittedBy,
            long? rootJobID,
            long? parentJobID)
        {
            LogOnCreateJob(workspaceID, relatedObjectArtifactID, taskType, SubmittedBy);

            _agentService.CreateQueueTableOnce();

            DataRow row = _dataProvider.CreateScheduledJob(
                workspaceID,
                relatedObjectArtifactID,
                correlationId,
                taskType,
                nextRunTime,
                AgentTypeInformation.AgentTypeID,
                null,
                null,
                jobDetails,
                0,
                SubmittedBy,
                rootJobID,
                parentJobID);
            return CreateJob(row);
        }

        public void DeleteJob(long jobID)
        {
            _dataProvider.DeleteJob(jobID);
        }

        public Job GetJob(long jobID)
        {
            _agentService.CreateQueueTableOnce();

            DataRow row = _dataProvider.GetJob(jobID);
            return CreateJob(row);
        }

        public Job GetScheduledJobs(int workspaceID, int relatedObjectArtifactID, string taskName)
        {
            return GetScheduledJobs(workspaceID, relatedObjectArtifactID, new List<string> { taskName }).FirstOrDefault();
        }

        public IEnumerable<Job> GetScheduledJobs(int workspaceID, int relatedObjectArtifactID, List<string> taskTypes)
        {
            LogOnGetScheduledJob(workspaceID, relatedObjectArtifactID, taskTypes);
            _agentService.CreateQueueTableOnce();

            using (DataTable dataTable = _dataProvider.GetJobs(workspaceID, relatedObjectArtifactID, taskTypes))
            {
                return dataTable.Rows.Cast<DataRow>().Select(row => new Job(row)).ToList();
            }
        }

        public IEnumerable<Job> GetAllScheduledJobs()
        {
            _agentService.CreateQueueTableOnce();

            using (DataTable dataTable = _dataProvider.GetAllJobs())
            {
                return dataTable.Rows.Cast<DataRow>().Select(row => new Job(row)).ToList();
            }
        }

        public void UpdateStopState(IList<long> jobIds, StopState state)
        {
            if (!jobIds.Any())
            {
                return;
            }

            int count = _dataProvider.UpdateStopState(jobIds, state);
            if (count == 0)
            {
                LogOnUpdateJobStopStateError(state, jobIds);
                throw new InvalidOperationException("Invalid operation. Job state failed to update.");
            }

            LogCompletedUpdatedJobStopState(jobIds, state, count);
        }

        public IList<Job> GetJobs(long integrationPointId)
        {
            LogOnGetJobs(integrationPointId);
            using (DataTable data = _dataProvider.GetJobsByIntegrationPointId(integrationPointId))
            {
                return data.Rows.Cast<DataRow>().Select(row => new Job(row)).ToList();
            }
        }

        public void UpdateJobDetails(Job job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            LogUpdateJobDetails(job);
            _dataProvider.UpdateJobDetails(job.JobId, job.JobDetails);
        }

        public void FinalizeDrainStoppedJob(Job job)
        {
            _dataProvider.UnlockJob(job.JobId, StopState.DrainStopped);
            _log.LogInformation("Finished Drain-Stop finalization of Job with ID: {jobId} - JobInfo: {jobInfo}", job.JobId, job.RemoveSensitiveData());
        }

        public void UnlockJob(Job job)
        {
            _dataProvider.UnlockJob(job.JobId, StopState.None);
            _log.LogInformation("Unlocking Job with ID: {jobId} - JobInfo: {jobInfo}", job.JobId, job.RemoveSensitiveData());
        }

        private void CreateNewAndDeleteOldScheduledJob(
            Job job,
            string correlationId,
            IScheduleRule scheduleRule,
            string jobDetails)
        {
            LogOnCreateJob(job.WorkspaceID, job.RelatedObjectArtifactID, job.TaskType, job.SubmittedBy);

            DateTime? nextRunTime = null;
            if (scheduleRule != null)
            {
                nextRunTime = scheduleRule.GetNextUtcRunDateTime(job.NextRunTime);
            }
            if (nextRunTime.HasValue)
            {
                _dataProvider.CreateNewAndDeleteOldScheduledJob(
                    job.JobId,
                    job.WorkspaceID,
                    job.RelatedObjectArtifactID,
                    correlationId,
                    job.TaskType,
                    nextRunTime.Value,
                    AgentTypeInformation.AgentTypeID,
                    scheduleRule.GetType().AssemblyQualifiedName,
                    scheduleRule.ToSerializedString(),
                    jobDetails,
                    0,
                    job.SubmittedBy,
                    job.RootJobId,
                    job.ParentJobId);
            }
            else
            {
                throw new IntegrationPointsException($"Try to create new scheduled job without any rule specified. Previous Job Id: {job.JobId}");
            }

            LogOnCreatedScheduledJobBasedOnOldJob(job, nextRunTime);
        }

        private Job CreateJob(DataRow row)
        {
            Job job = null;
            if (row != null)
            {
                job = new Job(row);
                if (string.IsNullOrWhiteSpace(job.CorrelationID))
                {
                    job.CorrelationID = Guid.NewGuid().ToString();
                }
            }

            return job;
        }

        #region Logging

        private void LogJobInformation(Job job, int agentId)
        {
            _log.LogInformation("Job ID {jobId} has been picked up from the queue by Agent ID {agentId}. Job Information: {@job}", job.JobId, agentId, job.RemoveSensitiveData());
        }

        private void LogUpdateJobDetails(Job job)
        {
            _log.LogInformation("Attempting to update JobDetails for job with ID: {jobId} - JobInfo: {@jobInfo}", job.JobId, job.RemoveSensitiveData());
        }

        private void LogOnFinalizeJob(long jobJobId, string jobJobDetails, TaskResult taskResult)
        {
            _log.LogInformation(
                "Attempting to finalize job with ID: ({jobid}) in {TypeName}. Exceptions: {Exceptions}",
                jobJobId,
                nameof(JobService),
                JsonConvert.SerializeObject(taskResult.Exceptions));
        }

        private void LogOnCreateJob(int workspaceId, int relatedObjectArtifactId, string taskType, int submittedBy)
        {
            _log.LogInformation(
                "Attempting to create Job in {service} " +
                                "WorkspaceID: {workspaceId} " +
                                "RelatedObjectArtifactID: {relatedObjectArtifactId} " +
                                "Task types: {taskType} " +
                                "Submitted by: {submittedBy}",
                nameof(JobService),
                workspaceId,
                relatedObjectArtifactId,
                taskType,
                submittedBy);
        }

        private void LogOnGetScheduledJob(int workspaceId, int relatedObjectArtifactID, List<string> taskTypes)
        {
            _log.LogInformation(
                "Attempting to get scheduledJobs in {TypeName}. WorkspaceId: ({WorkspaceId}), RelatedObjectArtifactID: ({RelatedObjectArtifactID}). Task types: {TaskTypes}",
                nameof(JobService),
                workspaceId,
                relatedObjectArtifactID,
                string.Join(",", taskTypes));
        }

        private void LogOnUpdateJobStopStateError(StopState state, IList<long> jobIds)
        {
            _log.LogError(
                "An error occured during update of stop states of jobs with IDs ({jobIds}) to state {state} in {TypeName}",
                string.Join(",", jobIds), state, nameof(JobService));
        }

        private void LogCompletedUpdatedJobStopState(IList<long> jobIds, StopState state, int updatedCount)
        {
            _log.LogInformation(
                "Jobs {count} count have been updated with StopState {stopState}. Updated Jobs: {jobs}. AllJobsWereUpdated: {wereAllUpdated}",
                updatedCount,
                state,
                string.Join(",", jobIds),
                jobIds.Count == updatedCount);
        }

        private void LogOnGetJobs(long integrationPointId)
        {
            _log.LogInformation(
                "Attempting to retrieve jobs for Integration Point with ID: {integrationPointID} in {TypeName}",
                integrationPointId,
                nameof(JobService));
        }

        private void LogOnCreatedScheduledJob(Job job)
        {
            _log.LogInformation("Scheduled Job has been created:\n {job}", job.ToString());
        }

        private void LogOnCreatedScheduledJobBasedOnOldJob(
            Job job,
            DateTime? nextRunTime)
        {
            _log.LogInformation(
                "New scheduled job has been created based on OldJobId {oldJobId} with parameters:" +
                                "WorkspaceId: {workspaceId}, " +
                                "RelatedObjectId: {relatedObjectArtifactId}, " +
                                "TaskType: {taskType}, " +
                                "NextRunTime: {nextRunTime}, " +
                                "SubmitedBy: {submitedBy}, " +
                                "RootJobId: {rootJobId}, " +
                                "ParentJobId: {parentJobId}",
                job.JobId,
                job.WorkspaceID,
                job.RelatedObjectArtifactID,
                job.TaskType,
                nextRunTime,
                job.SubmittedBy,
                job.RootJobId,
                job.ParentJobId);
        }

        #endregion
    }
}
