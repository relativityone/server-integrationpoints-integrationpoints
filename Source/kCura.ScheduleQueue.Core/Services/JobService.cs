using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using kCura.ScheduleQueue.Core.Properties;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Services
{
	public class JobService : IJobService
	{
		private readonly IAPILog _logger;

		public JobService(IAgentService agentService, IHelper dbHelper)
		{
			this.AgentService = agentService;
			this.QDBContext = new QueueDBContext(dbHelper, this.AgentService.QueueTable);
			_logger = dbHelper.GetLoggerFactory().GetLogger().ForContext<JobService>();
		}

		public IAgentService AgentService { get; private set; }
		public IQueueDBContext QDBContext { get; private set; }

		public AgentTypeInformation AgentTypeInformation
		{
			get { return AgentService.AgentTypeInformation; }
		}

		public Job GetNextQueueJob(IEnumerable<int> resourceGroupIds, int agentID)
		{
			Job job = null;
			DataRow row = new GetNextJob(QDBContext).Execute(agentID, AgentTypeInformation.AgentTypeID,
				resourceGroupIds.ToArray());
			if (row != null)
			{
				job = new Job(row);
			}
			return job;
		}

		public ITask GetTask(Job job)
		{
			//TODO: possibly implement generic way through reflection
			return null;
		}

		public DateTime? GetJobNextUtcRunDateTime(Job job, IScheduleRuleFactory scheduleRuleFactory, TaskResult taskResult)
		{
			if (job == null)
				return null;
			FinalizeJobResult result = new FinalizeJobResult();

			IScheduleRule scheduleRule = scheduleRuleFactory.Deserialize(job);
			DateTime? nextUtcRunDateTime = null;
			if (scheduleRule != null)
			{
#if TIME_MACHINE
				scheduleRule.TimeService = new TimeMachineService(job.WorkspaceID);
#endif
				nextUtcRunDateTime = scheduleRule.GetNextUTCRunDateTime(DateTime.UtcNow, taskResult.Status);
			}
			return nextUtcRunDateTime;
		}

		public FinalizeJobResult FinalizeJob(Job job, IScheduleRuleFactory scheduleRuleFactory, TaskResult taskResult)
		{
			if (job == null)
			{
				return new FinalizeJobResult() {JobState = JobLogState.Finished};
			}

			LogOnFinalizeJob(job.JobId, job.JobDetails);

			FinalizeJobResult result = new FinalizeJobResult();

			DateTime? nextUtcRunDateTime = GetJobNextUtcRunDateTime(job, scheduleRuleFactory, taskResult);
			if (nextUtcRunDateTime.HasValue)
			{
				new UpdateScheduledJob(QDBContext).Execute(job.JobId, nextUtcRunDateTime.Value);
				result.JobState = JobLogState.Modified;
				result.Details = string.Format("Job is re-scheduled for {0}", nextUtcRunDateTime.ToString());
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
			new UnlockScheduledJob(QDBContext).Execute(agentID);
		}

		public Job CreateJob(int workspaceID, int relatedObjectArtifactID, string taskType,
			IScheduleRule scheduleRule, string jobDetails, int SubmittedBy, long? rootJobID, long? parentJobID)
		{
			LogOnCreateJob(workspaceID, relatedObjectArtifactID, taskType, jobDetails, SubmittedBy);
			AgentService.CreateQueueTableOnce();

			Job job = null;
#if TIME_MACHINE
			scheduleRule.TimeService = new TimeMachineService(workspaceID);
#endif
			DateTime? nextRunTime = scheduleRule.GetNextUTCRunDateTime(null, null);
			string serializedScheduleRule = scheduleRule.ToSerializedString();
			if (nextRunTime.HasValue)
			{
				DataRow row = new CreateScheduledJob(QDBContext).Execute(
					workspaceID,
					relatedObjectArtifactID,
					taskType,
					nextRunTime.Value,
					AgentTypeInformation.AgentTypeID,
					scheduleRule.GetType().AssemblyQualifiedName,
					serializedScheduleRule,
					jobDetails,
					0,
					SubmittedBy,
					rootJobID,
					parentJobID);

				if (row != null) job = new Job(row);
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

			Job job = null;
			DataRow row = new CreateScheduledJob(QDBContext).Execute(
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

			if (row != null) job = new Job(row);

			return job;
		}

		public void DeleteJob(long jobID)
		{
			LogOnDeleteJob(jobID);
			new DeleteJob(QDBContext).Execute(jobID);
		}

		public Job GetJob(long jobID)
		{
			LogOnGetJob(jobID);

			AgentService.CreateQueueTableOnce();

			Job job = null;
			DataRow row = new GetJob(QDBContext).Execute(jobID);
			if (row != null) job = new Job(row);

			return job;
		}

		public Job GetScheduledJobs(int workspaceID, int relatedObjectArtifactID, string taskName)
		{
			LogOnGetScheduledJob(workspaceID, relatedObjectArtifactID, new List<string>() {taskName});
			return Execute(workspaceID, relatedObjectArtifactID, new List<string> {taskName})?.FirstOrDefault();
		}

		public IEnumerable<Job> GetScheduledJobs(int workspaceID, int relatedObjectArtifactID, List<string> taskTypes)
		{
			LogOnGetScheduledJob(workspaceID, relatedObjectArtifactID, taskTypes);
			return Execute(workspaceID, relatedObjectArtifactID, taskTypes);
		}

		public void UpdateStopState(IList<long> jobIds, StopState state)
		{
			LogOnUpdateJobStopState(state, jobIds);

			if (jobIds.Any())
			{
				string query = String.Format(Resources.UpdateStopState, QDBContext.TableName, String.Join(",", jobIds.Distinct()));
				List<SqlParameter> sqlParams = new List<SqlParameter> {new SqlParameter("@State", (int) state)};
				int count = QDBContext.EddsDBContext.ExecuteNonQuerySQLStatement(query, sqlParams);
				if (count == 0)
				{
					LogOnUpdateJobStopStateError(state, jobIds);
					throw new InvalidOperationException("Invalid operation. Job state failed to update.");
				}
			}
		}

		public IList<Job> GetJobs(long integrationPointId)
		{
			LogOnGetJobs(integrationPointId);

			List<Job> jobs = new List<Job>();
			string query = $@"SELECT [JobID]
	  ,[RootJobID]
	  ,[ParentJobID]
	  ,[AgentTypeID]
	  ,[LockedByAgentID]
	  ,[WorkspaceID]
	  ,[RelatedObjectArtifactID]
	  ,[TaskType]
	  ,[NextRunTime]
	  ,[LastRunTime]
	  ,[ScheduleRuleType]
	  ,[ScheduleRule]
	  ,[JobDetails]
	  ,[JobFlags]
	  ,[SubmittedDate]
	  ,[SubmittedBy]
	  ,[StopState] FROM [eddsdbo].[{QDBContext.TableName}] WHERE RelatedObjectArtifactID = @RelatedObjectArtifactID";

			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(new SqlParameter("@RelatedObjectArtifactID", integrationPointId));
			using (DataTable data = QDBContext.EddsDBContext.ExecuteSqlStatementAsDataTable(query, sqlParams))
			{
				foreach (DataRow row in data.Rows)
				{
					var job = new Job(row);
					jobs.Add(job);
				}
			}
			return jobs;
		}

		public void CleanupJobQueueTable()
		{
			LogOnCleanJobQueTable();
			var cleanupJobQueueTable = new CleanupJobQueueTable(QDBContext);
			cleanupJobQueueTable.Execute();
		}

		private IEnumerable<Job> Execute(int workspaceID, int relatedObjectArtifactID, List<string> taskTypes)
		{
			AgentService.CreateQueueTableOnce();

			List<DataRow> rows = new GetJob(QDBContext).Execute(workspaceID, relatedObjectArtifactID, taskTypes);
			return rows?.Select(row => new Job(row)) ?? Enumerable.Empty<Job>();
		}

		#region Logging

		private void LogOnFinalizeJob(long jobJobId, string jobJobDetails)
		{
			_logger.LogInformation("Attempting to finalize job with ID: ({jobid}) in {TypeName}. Job details: {Jobdetails}",
				jobJobId, nameof(JobService), jobJobDetails);
		}

		private void LogOnUnlockJobs(int agentId)
		{
			_logger.LogInformation("Attempting to unlock scheduled jobs for Agent with ID: ({agentId} in {TypeName})", agentId, nameof(JobService));
		}

		private void LogOnCreateJob(int workspaceID, int relatedObjectArtifactID, string taskType, string jobDetails, int submittedBy)
		{
			string message =
				$"Attempting to create Job in {nameof(JobService)}.{Environment.NewLine}" +
				$"WorkspaceId: ({workspaceID}).{Environment.NewLine}" +
				$" RelatedObjectArtifactID: ({relatedObjectArtifactID}).{Environment.NewLine}" +
				$" Task types: {taskType}.{Environment.NewLine}" +
				$" Job details: ({jobDetails}).{Environment.NewLine}" +
				$" Submitted by: {submittedBy}";
			_logger.LogInformation(message);
		}

		private void LogOnDeleteJob(long jobId)
		{
			_logger.LogInformation("Attempting to delete Job with ID: ({JobId}) in {TypeName}", jobId, nameof(JobService));
		}

		private void LogOnGetJob(long jobId)
		{
			_logger.LogInformation("Attempting to retrieve Job with ID: ({JobId}) in {TypeName}", jobId, nameof(JobService));
		}

		private void LogOnGetScheduledJob(int workspaceId, int relatedObjectArtifactID, List<string> taskTypes)
		{
			_logger.LogInformation(
				"Attempting to get scheduledJobs in {TypeName}. WorkspaceId: ({WorkspaceId}), RelatedObjectArtifactID: ({RelatedObjectArtifactID}). Task types: {TaskTypes}",
				nameof(JobService), workspaceId, relatedObjectArtifactID, string.Join(",", taskTypes));
		}

		private void LogOnUpdateJobStopStateError(StopState state, IList<long> jobIds)
		{
			_logger.LogError(
				"An error occured during update of stop states of jobs with IDs ({jobIds}) to state {state} in {TypeName}",
				string.Join(",", jobIds), state, nameof(JobService));
		}

		private void LogOnUpdateJobStopState(StopState state, IList<long> jobIds)
		{
			_logger.LogInformation("Attempting to update Stop state of jobs with IDs ({jobIds}) to {state} state in {TypeName}",
				string.Join(",", jobIds), state.ToString(), nameof(JobService));
		}

		private void LogOnGetJobs(long integrationPointId)
		{
			_logger.LogInformation(
				"Attempting to retrieve jobs for Integration Point with ID: {integrationPointID} in {TypeName}", integrationPointId,
				nameof(JobService));
		}

		private void LogOnCleanJobQueTable()
		{
			_logger.LogInformation("Attempting to Cleanup Job queue table in {TypeName}", nameof(JobService));
		}

		#endregion
	}
}