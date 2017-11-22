using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Services
{
	public class JobService : IJobService
	{
		private readonly IAPILog _log;

		public JobService(IAgentService agentService, IJobServiceDataProvider dataProvider, IHelper dbHelper)
		{
			AgentService = agentService;
			_log = dbHelper.GetLoggerFactory().GetLogger().ForContext<JobService>();
			DataProvider = dataProvider;
		}

		protected IJobServiceDataProvider DataProvider { get; set; }

		public IAgentService AgentService { get; }

		public AgentTypeInformation AgentTypeInformation => AgentService.AgentTypeInformation;

		public Job GetNextQueueJob(IEnumerable<int> resourceGroupIds, int agentID)
		{
			if (resourceGroupIds == null)
			{
				throw new ArgumentNullException(nameof(resourceGroupIds));
			}
			int[] resurceGroupIdsArray = resourceGroupIds.ToArray();
			if (resurceGroupIdsArray.Length == 0)
			{
				throw new ArgumentException($"Did not find any resource group ids for agent with id '{agentID}'");
			}

			DataRow row = DataProvider.GetNextQueueJob(agentID, AgentTypeInformation.AgentTypeID, resurceGroupIdsArray);
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

			LogOnFinalizeJob(job.JobId, job.JobDetails);

			var result = new FinalizeJobResult();

			DateTime? nextUtcRunDateTime = GetJobNextUtcRunDateTime(job, scheduleRuleFactory, taskResult);
			if (nextUtcRunDateTime.HasValue)
			{
				DataProvider.UpdateScheduledJob(job.JobId, nextUtcRunDateTime.Value);
				result.JobState = JobLogState.Modified;
				result.Details = string.Format("Job is re-scheduled for {0}", nextUtcRunDateTime);
			}
			else
			{
				DeleteJob(job.JobId);
				result.JobState = JobLogState.Deleted;
			}
			return result;
		}

		public void UnlockJobs(int agentID)
		{
			LogOnUnlockJobs(agentID);
			DataProvider.UnlockScheduledJob(agentID);
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
		}

		public IList<Job> GetJobs(long integrationPointId)
		{
			LogOnGetJobs(integrationPointId);
			using (DataTable data = DataProvider.GetJobsByIntegrationPointId(integrationPointId))
			{
				return data.Rows.Cast<DataRow>().Select(row => new Job(row)).ToList();
			}
		}

		public void CleanupJobQueueTable()
		{
			LogOnCleanJobQueTable();
			DataProvider.CleanupJobQueueTable();
		}

		#region Logging

		public void LogOnFinalizeJob(long jobJobId, string jobJobDetails)
		{
			_log.LogInformation("Attempting to finalize job with ID: ({jobid}) in {TypeName}. Job details: {Jobdetails}",
				jobJobId, nameof(JobService), jobJobDetails);
		}

		public void LogOnUnlockJobs(int agentId)
		{
			_log.LogInformation("Attempting to unlock scheduled jobs for Agent with ID: ({agentId} in {TypeName})", agentId, nameof(JobService));
		}

		public void LogOnCreateJob(int workspaceId, int relatedObjectArtifactId, string taskType, string jobDetails, int submittedBy)
		{
			string message =
				$"Attempting to create Job in {nameof(JobService)}.{Environment.NewLine}" +
				$"WorkspaceId: ({workspaceId}).{Environment.NewLine}" +
				$" RelatedObjectArtifactID: ({relatedObjectArtifactId}).{Environment.NewLine}" +
				$" Task types: {taskType}.{Environment.NewLine}" +
				$" Job details: ({jobDetails}).{Environment.NewLine}" +
				$" Submitted by: {submittedBy}";
			_log.LogInformation(message);
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
			_log.LogInformation("Attempting to get all scheduledJobs in {TypeName}.", nameof(JobService));
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

		public void LogOnGetJobs(long integrationPointId)
		{
			_log.LogInformation(
				"Attempting to retrieve jobs for Integration Point with ID: {integrationPointID} in {TypeName}", integrationPointId,
				nameof(JobService));
		}

		public void LogOnCleanJobQueTable()
		{
			_log.LogInformation("Attempting to Cleanup Job queue table in {TypeName}", nameof(JobService));
		}

		#endregion
	}
}