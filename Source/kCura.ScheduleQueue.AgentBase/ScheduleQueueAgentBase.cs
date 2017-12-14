using System;
using System.Collections.Generic;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;
using kCura.IntegrationPoints.Domain.Logging;

namespace kCura.ScheduleQueue.AgentBase
{
	public delegate void AgentLoggingEventHandler(LogCategory category, string message, string detailmessage);

	public delegate void JobLoggingEventHandler(Job job, JobLogState state, string details = null);

	public delegate void ExceptionEventHandler(Job job, ITask task, Exception exception);

	public abstract class ScheduleQueueAgentBase : Agent.AgentBase, ITaskFactory
	{
		public virtual event ExceptionEventHandler JobExecutionError;

		private readonly Guid agentGuid;
		private IJobService jobService;

		protected IAPILog Logger { get; set; }

		#region Constants

		private const string PROCESSING_JOB_MESSAGE_TEMPLATE = "Processing Job ID : {0} : Workspace ID {1} : Job Type {2}";
		private const string START_PROCESSING_JOB_MESSAGE_TEMPLATE = "Started : " + PROCESSING_JOB_MESSAGE_TEMPLATE;
		private const string FINISHED_PROCESSING_JOB_MESSAGE_TEMPLATE = "Finished : " + PROCESSING_JOB_MESSAGE_TEMPLATE;
		private const int MAX_MESSAGE_LENGTH = 10000;

		#endregion

		public ScheduleQueueAgentBase(Guid agentGuid,
			IAgentService agentService = null, IJobService jobService = null,
			IScheduleRuleFactory scheduleRuleFactory = null)
		{
			this.agentGuid = agentGuid;
			this.AgentService = agentService;
			this.jobService = jobService;
			this.ScheduleRuleFactory = scheduleRuleFactory ?? new DefaultScheduleRuleFactory();
		}

		protected virtual void Initialize()
		{
			//Logger cannot be initialized in constructor because Helper from Agent.Base is initialized later on
			Logger = Helper.GetLoggerFactory().GetLogger().ForContext<ScheduleQueueAgentBase>();

			NotifyAgentTab(20, LogCategory.Debug, "Initialize Agent core services");

			if (this.AgentService == null)
			{
				this.AgentService = new AgentService(base.Helper, agentGuid);
			}

			if (this.jobService == null)
			{
				this.jobService = new JobService(AgentService, new JobServiceDataProvider(AgentService, Helper), Helper);
			}
		}

		public IAgentService AgentService { get; private set; }
		public IScheduleRuleFactory ScheduleRuleFactory { get; private set; }

		public abstract ITask GetTask(Job job);

		public sealed override void Execute()
		{
			NotifyAgentTab(10, LogCategory.Info, "Started.");

			try
			{
				Initialize();
				InitializeManagerConfigSettingsFactory();
				CheckQueueTable();
				ProcessQueueJobs();
				CleanupQueueJobs();
			}
			catch (Exception ex)
			{
				NotifyAgentTab(20, LogCategory.Warn, $"{ex.Message} {ex.StackTrace}");
				LogExecuteError(ex);
			}

			NotifyAgentTab(10, LogCategory.Info, "Completed.");
			LogExecuteComplete();
		}

		protected virtual void ReleaseTask(ITask task)
		{
		}

		private void InitializeManagerConfigSettingsFactory()
		{
			NotifyAgentTab(20, LogCategory.Info, "Initialize Config Settings factory");
			LogOnInitializeManagerConfigSettingsFactory();
			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(base.Helper);
		}

		private void CheckQueueTable()
		{
			NotifyAgentTab(20, LogCategory.Info, "Check Schedule Agent Queue table exists");

			AgentService.InstallQueueTable();
		}

		protected virtual IEnumerable<int> GetListOfResourceGroupIDs() // this method was added for unit testing purpose
		{
			return GetResourceGroupIDs();
		}

		public void ProcessQueueJobs()
		{
			if (!Enabled)
			{
				NotifyAgentTab(20, LogCategory.Info, "Agent was disabled. Terminating job processing task.");
				return;
			}
			NotifyAgentTab(20, LogCategory.Info, "Checking for active jobs in Schedule Agent Queue table");

			Job nextJob = jobService.GetNextQueueJob(GetListOfResourceGroupIDs(), base.AgentID);

			if (nextJob == null)
			{
				Logger.LogDebug("No active job found in Schedule Agent Queue table");
			}
			while (nextJob != null)
			{
				ProcessJob(nextJob);

				nextJob = jobService.GetNextQueueJob(GetListOfResourceGroupIDs(), base.AgentID);
			}
			if (base.ToBeRemoved)
			{
				jobService.UnlockJobs(base.AgentID);
			}
		}

		private void ProcessJob(Job nextJob)
		{
			string agentMessage = string.Format(START_PROCESSING_JOB_MESSAGE_TEMPLATE, nextJob.JobId, nextJob.WorkspaceID,
				nextJob.TaskType);
			NotifyAgentTab(1, LogCategory.Info, agentMessage);
			LogOnStartJobProcessing(agentMessage, nextJob.JobId, nextJob.WorkspaceID, nextJob.TaskType);

			TaskResult taskResult = ExecuteTask(nextJob);

			FinalizeJob(nextJob, taskResult);
		}

		private TaskResult ExecuteTask(Job job)
		{
			TaskResult result = new TaskResult() { Status = TaskStatusEnum.Success, Exceptions = null };
			ITask task = null;
			try
			{
				LogJobState(job, JobLogState.Started);
				LogStartingExecuteTask(job);
				task = GetTask(job);
				if (task == null)
				{
					throw new Exception("Could not find corresponding Task. ");
				}

				StartTask(job, task);

				LogJobState(job, JobLogState.Finished);
				string msg = string.Format(FINISHED_PROCESSING_JOB_MESSAGE_TEMPLATE, job.JobId, job.WorkspaceID, job.TaskType);
				NotifyAgentTab(1, LogCategory.Info, msg);
				LogFinishingExecuteTask(job);
			}
			catch (Exception ex)
			{
				result.Status = TaskStatusEnum.Fail;
				result.Exceptions = new List<Exception>() { ex };
				RaiseJobExecutionErrorEvent(job,task,ex);
			}
			finally
			{
				ReleaseTask(task);
			}
			return result;
		}

		private void StartTask(Job job, ITask task)
		{
			var context = new AgentCorrelationContext
			{
				JobId = job.JobId,
				RootJobId = job.RootJobId,
				WorkspaceId = job.WorkspaceID,
				UserId = job.SubmittedBy,
				ActionName = task.GetType().Name
			};

			using (LogContextHelper.CreateAgentLogContext(context))
			using (Logger.LogContextPushProperties(context))
			{
				Logger.LogInformation("StartTask - Before Execute");
				task.Execute(job);
				Logger.LogInformation("StartTask - After Execute");
			}
		}

		private void FinalizeJob(Job job, TaskResult taskResult)
		{
			try
			{
				FinalizeJobResult result = jobService.FinalizeJob(job, this.ScheduleRuleFactory, taskResult);
				LogJobState(job, result.JobState, null, result.Details);
				LogFinalizeJob(job, result);
			}
			catch (Exception ex)
			{
				RaiseJobExecutionErrorEvent(job, null, ex);
				LogFinalizeJobError(job, ex);
			}
		}

		private void CleanupQueueJobs()
		{
			NotifyAgentTab(20, LogCategory.Info, "Cleanup jobs");
			LogOnCleanupJobs();

			jobService.CleanupJobQueueTable();
		}

		protected void NotifyAgentTab(int level, LogCategory category, string message,
			string detailmessage = null)
		{
			string msg = message.Substring(0, Math.Min(message.Length, MAX_MESSAGE_LENGTH));
			switch (category)
			{
				case LogCategory.Info:
				case LogCategory.Debug:
					base.RaiseMessage(msg, level);
					break;
				case LogCategory.Exception:
					base.RaiseError(msg, detailmessage);
					break;
				case LogCategory.Warn:
					base.RaiseWarning(msg);
					break;
				default:
					throw new ArgumentOutOfRangeException("category");
			}
		}

		protected void RaiseJobExecutionErrorEvent(Job job, ITask task, Exception exception)
		{
			JobExecutionError?.Invoke(job,task,exception);
		}

		protected abstract void LogJobState(Job job, JobLogState state, Exception exception = null,
			string details = null);

		#region Logging

		private void LogOnInitializeManagerConfigSettingsFactory()
		{
			Logger.LogInformation("Attempting to initialize Config Settings factory in {TypeName}",
				nameof(ScheduleQueueAgentBase));
		}

		private void LogOnCleanupJobs()
		{
			Logger.LogInformation("Attempting to cleanup jobs in {TypeName}", nameof(ScheduleQueueAgentBase));
		}

		private void LogExecuteComplete()
		{
			Logger.LogInformation("Execution of scheduled jobs completed in {TypeName}", nameof(ScheduleQueueAgentBase));
		}

		private void LogExecuteError(Exception ex)
		{
			Logger.LogError(ex, "An error occured during executing scheduled jobs in {TypeName}", nameof(ScheduleQueueAgentBase));
		}

		private void LogOnStartJobProcessing(string agentMessage, long nextJobJobId, int nextJobWorkspaceId,
			string nextJobTaskType)
		{
			Logger.LogInformation(agentMessage, nextJobJobId, nextJobWorkspaceId, nextJobTaskType);
		}

		private void LogStartingExecuteTask(Job job)
		{
			Logger.LogInformation("Attempting to execute Job with ID: {JobID} in {TypeName}", job.JobId,
				nameof(ScheduleQueueAgentBase));
		}

		private void LogFinishingExecuteTask(Job job)
		{
			Logger.LogInformation("Finished execution of Job with ID: {JobID} in {TypeName}", job.JobId,
				nameof(ScheduleQueueAgentBase));
		}


		private void LogFinalizeJob(Job job, FinalizeJobResult result)
		{
			string message = $"Finished Finalization of Job with ID: {job.JobId} in {nameof(ScheduleQueueAgentBase)}." +
							$"{Environment.NewLine}Job result: {result.JobState}," +
							$"{Environment.NewLine} Details: {result.Details}";

			Logger.LogInformation(message);
		}

		private void LogFinalizeJobError(Job job, Exception exception)
		{
			Logger.LogError(exception, "An error occured during finalization of Job with ID: {JobID} in {TypeName}", job.JobId,
				nameof(ScheduleQueueAgentBase));
		}

		#endregion
	}
}