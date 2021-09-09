using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Toggles;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.ScheduleQueue.Core.Services
{
	public class JobService : IJobService
	{
		private readonly IToggleProvider _toggleProvider;
		private readonly IAPILog _log;

		public JobService(IAgentService agentService, IJobServiceDataProvider dataProvider, IToggleProvider toggleProvider, IHelper dbHelper)
		{
			AgentService = agentService;
			_log = dbHelper.GetLoggerFactory().GetLogger().ForContext<JobService>();
			DataProvider = dataProvider;
			_toggleProvider = toggleProvider;
		}

		protected IJobServiceDataProvider DataProvider { get; set; }

		public IAgentService AgentService { get; }

		public AgentTypeInformation AgentTypeInformation => AgentService.AgentTypeInformation;

		public Job GetNextQueueJob(IEnumerable<int> resourceGroupIds, int agentID)
		{
			_log.LogInformation("Get next job from the queue for Agent {agentId}.", agentID);

			DataRow row;

			if (_toggleProvider.IsEnabled<EnableKubernetesMode>())
			{
				row = DataProvider.GetNextQueueJob(agentID, AgentTypeInformation.AgentTypeID);
			}
			else
			{
				int[] resurceGroupIdsArray = resourceGroupIds?.ToArray() ?? Array.Empty<int>();

				if (resurceGroupIdsArray.Length == 0)
				{
					throw new ArgumentException($"Did not find any resource group ids for agent with id '{agentID}'");
				}

				row = DataProvider.GetNextQueueJob(agentID, AgentTypeInformation.AgentTypeID, resurceGroupIdsArray);
			}

			return CreateJob(row);
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

		public FinalizeJobResult FinalizeJob(Job job, IScheduleRuleFactory scheduleRuleFactory, TaskResult taskResult)
		{
			if (scheduleRuleFactory == null)
			{
				throw new ArgumentNullException(nameof(scheduleRuleFactory));
			}

			if (job == null)
			{
				return new FinalizeJobResult {JobState = JobLogState.Finished};
			}

			LogOnFinalizeJob(job.JobId, job.JobDetails, taskResult);

			var result = new FinalizeJobResult();

			DateTime? nextUtcRunDateTime = GetJobNextUtcRunDateTime(job, scheduleRuleFactory, taskResult);
			if (nextUtcRunDateTime.HasValue)
			{
				_log.LogInformation("Job {jobId} was scheduled with following details: " +
				                    "NextRunTime - {nextRunTime} " +
				                    "ScheduleRule - {scheduleRule}",
					job.JobId, nextUtcRunDateTime, job.SerializedScheduleRule);

				TaskParameters taskParameters = new TaskParameters()
				{
					BatchInstance = Guid.NewGuid()
				};
				string jobDeatils = new JSONSerializer().Serialize(taskParameters);
				CreateNewAndDeleteOldScheduledJob(job.JobId, job.WorkspaceID, job.RelatedObjectArtifactID, job.TaskType, scheduleRuleFactory.Deserialize(job),
					jobDeatils, job.SubmittedBy, job.RootJobId,
					job.ParentJobId);
			}
			else
			{
				_log.LogInformation("Deleting job {jobId} from the queue since it wasn't scheduled...");

				DeleteJob(job.JobId);
			}

			result.JobState = JobLogState.Deleted;
			return result;
		}

		public void UnlockJobs(int agentID)
		{
			LogOnUnlockJobs(agentID);
			DataProvider.UnlockScheduledJob(agentID);
		}

		public void CreateNewAndDeleteOldScheduledJob(long oldJobId, int workspaceID, int relatedObjectArtifactID, string taskType,
			IScheduleRule scheduleRule, string jobDetails, int submittedBy, long? rootJobID, long? parentJobID)
		{
			LogOnCreateJob(workspaceID, relatedObjectArtifactID, taskType, jobDetails, submittedBy);

			DateTime? nextRunTime = scheduleRule.GetNextUTCRunDateTime();
			if (nextRunTime.HasValue)
			{
				DataProvider.CreateNewAndDeleteOldScheduledJob(
					oldJobId,
					workspaceID,
					relatedObjectArtifactID,
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

		public Job CreateJob(int workspaceID, int relatedObjectArtifactID, string taskType,
			IScheduleRule scheduleRule, string jobDetails, int SubmittedBy, long? rootJobID, long? parentJobID)
		{
			LogOnCreateJob(workspaceID, relatedObjectArtifactID, taskType, jobDetails, SubmittedBy);
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

		public Job CreateJob(int workspaceID, int relatedObjectArtifactID, string taskType,
			DateTime nextRunTime, string jobDetails, int SubmittedBy, long? rootJobID, long? parentJobID)
		{
			LogOnCreateJob(workspaceID, relatedObjectArtifactID, taskType, jobDetails, SubmittedBy);

			AgentService.CreateQueueTableOnce();

			DataRow row = DataProvider.CreateScheduledJob(
				workspaceID,
				relatedObjectArtifactID,
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
			LogOnDeleteJob(jobID);
			DataProvider.DeleteJob(jobID);
		}

		public Job GetJob(long jobID)
		{
			LogOnGetJob(jobID);

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
			return GetScheduledJobs(workspaceID, relatedObjectArtifactID, new List<string> {taskName}).FirstOrDefault();
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
			LogOnGetAllScheduledJob();
			AgentService.CreateQueueTableOnce();

			using (DataTable dataTable = DataProvider.GetAllJobs())
			{
				return dataTable.Rows.Cast<DataRow>().Select(row => new Job(row)).ToList();
			}
		}

		public void UpdateStopState(IList<long> jobIds, StopState state)
		{
			LogOnUpdateJobStopState(state, jobIds);

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
			if(job == null)
			{
				throw new ArgumentNullException(nameof(job));
			}

			LogUpdateJobDetails(job.JobId);
			DataProvider.UpdateJobDetails(job.JobId, job.JobDetails);
		}
		
		public void CleanupJobQueueTable()
		{
			LogOnCleanJobQueTable();
			DataProvider.CleanupJobQueueTable();
		}

		public void FinalizeDrainStoppedJob(Job job)
		{
            UpdateStopState(new List<long>() { job.JobId }, StopState.DrainStopped);
            DataProvider.UnlockJob(job.JobId);
			_log.LogInformation("Finished Drain-Stop finalization of Job with ID: {jobId}", job.JobId);
		}

		#region Logging
		private void LogUpdateJobDetails(long jobId)
		{
			_log.LogInformation("Attempting to update JobDetails for job with ID: ({jobId})", jobId);
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

		public void LogOnCreateJob(int workspaceId, int relatedObjectArtifactId, string taskType, string jobDetails, int submittedBy)
		{
			_log.LogInformation("Attempting to create Job in {service} " +
			                    "WorkspaceID: {workspaceId} " +
			                    "RelatedObjectArtifactID: {relatedObjectArtifactId} " +
			                    "Task types: {taskType} " +
			                    "Submitted by: {submittedBy}",
				nameof(JobService), workspaceId, relatedObjectArtifactId, taskType, submittedBy);
		}

		public void LogOnDeleteJob(long jobId)
		{
			_log.LogInformation("Attempting to delete Job with ID: ({JobId}) in {TypeName}", jobId, nameof(JobService));
		}

		public void LogOnGetJob(long jobId)
		{
			_log.LogVerbose("Attempting to retrieve Job with ID: ({JobId}) in {TypeName}", jobId, nameof(JobService));
		}

		public void LogOnGetScheduledJob(int workspaceId, int relatedObjectArtifactID, List<string> taskTypes)
		{
			_log.LogInformation(
				"Attempting to get scheduledJobs in {TypeName}. WorkspaceId: ({WorkspaceId}), RelatedObjectArtifactID: ({RelatedObjectArtifactID}). Task types: {TaskTypes}",
				nameof(JobService), workspaceId, relatedObjectArtifactID, string.Join(",", taskTypes));
		}

		public void LogOnGetAllScheduledJob()
		{
			_log.LogDebug("Attempting to get all scheduledJobs in {TypeName}.", nameof(JobService));
		}

		public void LogOnUpdateJobStopStateError(StopState state, IList<long> jobIds)
		{
			_log.LogError(
				"An error occured during update of stop states of jobs with IDs ({jobIds}) to state {state} in {TypeName}",
				string.Join(",", jobIds), state, nameof(JobService));
		}

		public void LogOnUpdateJobStopState(StopState state, IList<long> jobIds)
		{
			_log.LogInformation("Attempting to update Stop state of jobs with IDs ({jobIds}) to {state} state in {TypeName}",
				string.Join(",", jobIds), state.ToString(), nameof(JobService));
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

		public void LogOnCleanJobQueTable()
		{
			_log.LogDebug("Attempting to Cleanup Job queue table in {TypeName}", nameof(JobService));
		}

		private void LogOnCreatedScheduledJob(Job job)
		{
			_log.LogInformation("Scheduled Job has been created:\n {@job}", job);
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