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

        public JobService(IAgentService agentService, IJobServiceDataProvider dataProvider, IKubernetesMode kubernetesMode, IHelper dbHelper)
        {
            _kubernetesMode = kubernetesMode;
            AgentService = agentService;
            _log = dbHelper.GetLoggerFactory().GetLogger().ForContext<JobService>();
            DataProvider = dataProvider;
            _serializer = new RipJsonSerializer(_log);
        }

        protected IJobServiceDataProvider DataProvider { get; set; }

        public IAgentService AgentService { get; }

        public AgentTypeInformation AgentTypeInformation => AgentService.AgentTypeInformation;

        public Job GetNextQueueJob(IEnumerable<int> resourceGroupIds, int agentID, long? rootJobId = null)
        {
            DataRow row;

            if (_kubernetesMode.IsEnabled())
            {
                row = DataProvider.GetNextQueueJob(agentID, AgentTypeInformation.AgentTypeID, rootJobId);
            }
            else
            {
                int[] resourceGroupIdsArray = resourceGroupIds?.ToArray() ?? Array.Empty<int>();

                if (resourceGroupIdsArray.Length == 0)
                {
                    throw new ArgumentException($"Did not find any resource group ids for agent with id '{agentID}'." +
                                                        " Please validate EnableKubernetesMode toggle value, current value: False");
                }

                row = DataProvider.GetNextQueueJob(agentID, AgentTypeInformation.AgentTypeID, resourceGroupIdsArray);
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

            DateTime? nextUtcRunDateTime = GetJobNextUtcRunDateTime(job, scheduleRuleFactory, taskResult);

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
                    "CorrelationID - {correlationID}",
                    job.JobId,
                    nextUtcRunDateTime,
                    job.ScheduleRule,
                    newJobCorrelationID);

                TaskParameters taskParameters = new TaskParameters()
                {
                    BatchInstance = newJobCorrelationID
                };
                string jobDetails = _serializer.Serialize(taskParameters);
                CreateNewAndDeleteOldScheduledJob(job.JobId, job.WorkspaceID, job.RelatedObjectArtifactID, newJobCorrelationID.ToString(), job.TaskType, scheduleRule, jobDetails, job.SubmittedBy, job.RootJobId, job.ParentJobId);
            }
            else
            {
                _log.LogInformation("Deleting job {jobId} from the queue - ShouldBreakSchedule {shouldBreakSchedule}, IsScheduled {isScheduledJob}",
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
#if TIME_MACHINE
                scheduleRule.TimeService = new TimeMachineService(job.WorkspaceID);
#endif
                nextUtcRunDateTime = scheduleRule.GetNextUTCRunDateTime();
            }

            _log.LogInformation("NextUtcRunDateTime has been calculated for {nextUtcRunDateTime}.", nextUtcRunDateTime);

            return nextUtcRunDateTime;
        }

        public void CreateNewAndDeleteOldScheduledJob(long oldJobId, int workspaceID, int relatedObjectArtifactID, 
            string correlationID, string taskType, IScheduleRule scheduleRule, string jobDetails, int submittedBy,
            long? rootJobID, long? parentJobID)
        {
            LogOnCreateJob(workspaceID, relatedObjectArtifactID, taskType, submittedBy);

            DateTime? nextRunTime = scheduleRule.GetNextUTCRunDateTime();
            if (nextRunTime.HasValue)
            {
                DataProvider.CreateNewAndDeleteOldScheduledJob(
                    oldJobId,
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
                    submittedBy,
                    rootJobID,
                    parentJobID);
            }
            else
            {
                throw new IntegrationPointsException($"Try to create new scheduled job without any rule specified. Previous Job Id: {oldJobId}");
            }

            LogOnCreatedScheduledJobBasedOnOldJob(oldJobId, workspaceID, relatedObjectArtifactID,
                taskType, submittedBy, rootJobID, parentJobID, nextRunTime);
        }

        public Job CreateJob(int workspaceID, int relatedObjectArtifactID, string correlationID, string taskType,
            IScheduleRule scheduleRule, string jobDetails, int SubmittedBy, long? rootJobID, long? parentJobID)
        {
            LogOnCreateJob(workspaceID, relatedObjectArtifactID, taskType, SubmittedBy);
            AgentService.CreateQueueTableOnce();

            Job job;
#if TIME_MACHINE
            scheduleRule.TimeService = new TimeMachineService(workspaceID);
#endif
            DateTime? nextRunTime = scheduleRule.GetNextUTCRunDateTime();
            if (nextRunTime.HasValue)
            {
                DataRow row = DataProvider.CreateScheduledJob(
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

        public Job CreateJob(int workspaceID, int relatedObjectArtifactID, string correlationId, string taskType,
            DateTime nextRunTime, string jobDetails, int SubmittedBy, long? rootJobID, long? parentJobID)
        {
            LogOnCreateJob(workspaceID, relatedObjectArtifactID, taskType, SubmittedBy);

            AgentService.CreateQueueTableOnce();

            DataRow row = DataProvider.CreateScheduledJob(
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
            DataProvider.DeleteJob(jobID);
        }

        public Job GetJob(long jobID)
        {
            AgentService.CreateQueueTableOnce();

            DataRow row = DataProvider.GetJob(jobID);
            return CreateJob(row);
        }

        private Job CreateJob(DataRow row)
        {
            return row != null ? new Job(row) : null;
        }

        public Job GetScheduledJobs(int workspaceID, int relatedObjectArtifactID, string taskName)
        {
            return GetScheduledJobs(workspaceID, relatedObjectArtifactID, new List<string> { taskName }).FirstOrDefault();
        }

        public IEnumerable<Job> GetScheduledJobs(int workspaceID, int relatedObjectArtifactID, List<string> taskTypes)
        {
            LogOnGetScheduledJob(workspaceID, relatedObjectArtifactID, taskTypes);
            AgentService.CreateQueueTableOnce();

            using (DataTable dataTable = DataProvider.GetJobs(workspaceID, relatedObjectArtifactID, taskTypes))
            {
                return dataTable.Rows.Cast<DataRow>().Select(row => new Job(row)).ToList();
            }
        }

        public IEnumerable<Job> GetAllScheduledJobs()
        {
            AgentService.CreateQueueTableOnce();

            using (DataTable dataTable = DataProvider.GetAllJobs())
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

            int count = DataProvider.UpdateStopState(jobIds, state);
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
            using (DataTable data = DataProvider.GetJobsByIntegrationPointId(integrationPointId))
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
            DataProvider.UpdateJobDetails(job.JobId, job.JobDetails);
        }

        public void FinalizeDrainStoppedJob(Job job)
        {
            DataProvider.UnlockJob(job.JobId, StopState.DrainStopped);
            _log.LogInformation("Finished Drain-Stop finalization of Job with ID: {jobId} - JobInfo: {jobInfo}", job.JobId, job.RemoveSensitiveData());
        }

        public void UnlockJob(Job job)
        {
            DataProvider.UnlockJob(job.JobId, StopState.None);
            _log.LogInformation("Unlocking Job with ID: {jobId} - JobInfo: {jobInfo}", job.JobId, job.RemoveSensitiveData());
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

        public void LogOnFinalizeJob(long jobJobId, string jobJobDetails, TaskResult taskResult)
        {
            _log.LogInformation("Attempting to finalize job with ID: ({jobid}) in {TypeName}. Exceptions: {Exceptions}",
                jobJobId, nameof(JobService), JsonConvert.SerializeObject(taskResult.Exceptions));
        }

        public void LogOnUnlockJobs(int agentId)
        {
            _log.LogInformation("Attempting to unlock scheduled jobs for Agent with ID: ({agentId} in {TypeName})", agentId, nameof(JobService));
        }

        public void LogOnCreateJob(int workspaceId, int relatedObjectArtifactId, string taskType, int submittedBy)
        {
            _log.LogInformation("Attempting to create Job in {service} " +
                                "WorkspaceID: {workspaceId} " +
                                "RelatedObjectArtifactID: {relatedObjectArtifactId} " +
                                "Task types: {taskType} " +
                                "Submitted by: {submittedBy}",
                nameof(JobService), workspaceId, relatedObjectArtifactId, taskType, submittedBy);
        }

        public void LogOnGetJob(long jobId)
        {
            _log.LogInformation("Attempting to retrieve Job with ID: ({JobId}) in {TypeName}", jobId, nameof(JobService));
        }

        public void LogOnGetScheduledJob(int workspaceId, int relatedObjectArtifactID, List<string> taskTypes)
        {
            _log.LogInformation(
                "Attempting to get scheduledJobs in {TypeName}. WorkspaceId: ({WorkspaceId}), RelatedObjectArtifactID: ({RelatedObjectArtifactID}). Task types: {TaskTypes}",
                nameof(JobService), workspaceId, relatedObjectArtifactID, string.Join(",", taskTypes));
        }

        public void LogOnUpdateJobStopStateError(StopState state, IList<long> jobIds)
        {
            _log.LogError(
                "An error occured during update of stop states of jobs with IDs ({jobIds}) to state {state} in {TypeName}",
                string.Join(",", jobIds), state, nameof(JobService));
        }

        private void LogCompletedUpdatedJobStopState(IList<long> jobIds, StopState state, int updatedCount)
        {
            _log.LogInformation("Jobs {count} count have been updated with StopState {stopState}. Updated Jobs: {jobs}. AllJobsWereUpdated: {wereAllUpdated}",
                updatedCount, state, string.Join(",", jobIds), jobIds?.Count == updatedCount);
        }

        public void LogOnGetJobs(long integrationPointId)
        {
            _log.LogInformation(
                "Attempting to retrieve jobs for Integration Point with ID: {integrationPointID} in {TypeName}", integrationPointId,
                nameof(JobService));
        }

        private void LogOnCreatedScheduledJob(Job job)
        {
            _log.LogInformation("Scheduled Job has been created:\n {job}", job.ToString());
        }

        private void LogOnCreatedScheduledJobBasedOnOldJob(long oldJobId, int workspaceID, int relatedObjectArtifactID,
            string taskType, int submittedBy, long? rootJobID, long? parentJobID, DateTime? nextRunTime)
        {
            _log.LogInformation("New scheduled job has been created based on OldJobId {oldJobId} with parameters:" +
                                "WorkspaceId: {workspaceId}, " +
                                "Integration Point: {relatedObjectArtifactId}, " +
                                "TaskType: {taskType}, " +
                                "NextRunTime: {nextRunTime}, " +
                                "SubmitedBy: {submitedBy}, " +
                                "RootJobId: {rootJobId}, " +
                                "ParentJobId: {parentJobId}",
                                oldJobId, workspaceID, relatedObjectArtifactID, taskType,
                                nextRunTime, submittedBy, rootJobID, parentJobID);
        }

        #endregion
    }
}
