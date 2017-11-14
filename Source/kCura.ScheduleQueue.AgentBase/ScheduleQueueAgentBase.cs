﻿using System;
using System.Collections.Generic;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Logging;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;

namespace kCura.ScheduleQueue.AgentBase
{
	public delegate void AgentLoggingEventHandler(LogCategory category, string message, string detailmessage);

	public delegate void JobLoggingEventHandler(Job job, JobLogState state, string details = null);

	public delegate void ExceptionEventHandler(Job job, Exception exception);

	public abstract class ScheduleQueueAgentBase : Agent.AgentBase, ITaskFactory
	{
		public event AgentLoggingEventHandler RaiseAgentLogEntry;
		public event JobLoggingEventHandler RaiseJobLogEntry;
		public event ExceptionEventHandler RaiseException;

		private Guid agentGuid = Guid.Empty;
		private IJobService jobService;
		private bool errorRaised;
		private IAPILog _logger;

		#region Constants

		private const string PROCESSING_JOB_MESSAGE_TEMPLATE = "Processing Job ID : {0} : Workspace ID {1} : Job Type {2}";
		private const string START_PROCESSING_JOB_MESSAGE_TEMPLATE = "Started : " + PROCESSING_JOB_MESSAGE_TEMPLATE;
		private const string FINISHED_PROCESSING_JOB_MESSAGE_TEMPLATE = "Finished : " + PROCESSING_JOB_MESSAGE_TEMPLATE;
		private const int MAX_MESSAGE_LENGTH = 10000;

		#endregion

		public ScheduleQueueAgentBase(Guid agentGuid,
			IDBContext dbContext = null,
			IAgentService agentService = null,
			IJobService jobService = null,
			IScheduleRuleFactory scheduleRuleFactory = null)
		{
			this.agentGuid = agentGuid;
			this.AgentService = agentService;
			this.jobService = jobService;
			this.ScheduleRuleFactory = scheduleRuleFactory;
			this.DBContext = dbContext;
			if (this.ScheduleRuleFactory == null)
			{
				this.ScheduleRuleFactory = new DefaultScheduleRuleFactory();
			}
		}

		protected virtual void Initialize()
		{
			//Logger cannot be initialized in constructor because Helper from Agent.Base is initialized later on
			_logger = Helper.GetLoggerFactory().GetLogger().ForContext<ScheduleQueueAgentBase>();

			OnRaiseAgentLogEntry(20, LogCategory.Debug, "Initialize Agent core services");
			
			if (this.DBContext == null)
			{
				this.DBContext = base.Helper.GetDBContext(-1);
			}

			if (this.AgentService == null)
			{
				this.AgentService = new AgentService(base.Helper, agentGuid);
			}

			if (this.jobService == null)
			{
			    this.jobService = new JobService(AgentService, new JobServiceDataProvider(AgentService, Helper), Helper);
			}
		}

		public IDBContext DBContext { get; private set; }
		public IAgentService AgentService { get; private set; }
		public IScheduleRuleFactory ScheduleRuleFactory { get; private set; }

	    public abstract ITask GetTask(Job job);

		public TimeProvider TimeProvider
		{
			get { return TimeProvider.Current; }
			set { TimeProvider.Current = value; }
		}

		public sealed override void Execute()
		{
			errorRaised = false;

			OnRaiseAgentLogEntry(10, LogCategory.Info, "Started.");

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
				OnRaiseAgentLogEntry(20, LogCategory.Warn, string.Format("{0} {1}", ex.Message, ex.StackTrace));
				LogOnExecuteError(ex);
			}

			if (errorRaised)
			{
				OnRaiseAgentLogEntry(10, LogCategory.Info, "Completed with errors.");
				LogOnExecuteCompleteWithErrors();
			}
			else
			{
				OnRaiseAgentLogEntry(10, LogCategory.Info, "Completed.");
				LogOnExecuteComplete();
			}
		}

		protected virtual void ReleaseTask(ITask task)
		{
		}

		private void InitializeManagerConfigSettingsFactory()
		{
			OnRaiseAgentLogEntry(20, LogCategory.Info, "Initialize Config Settings factory");
			LogOnInitializeManagerConfigSettingsFactory();
			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(base.Helper);
		}

		private void CheckQueueTable()
		{
			OnRaiseAgentLogEntry(20, LogCategory.Info, "Check Schedule Agent Queue table exists");

			AgentService.InstallQueueTable();
		}

		public void ProcessQueueJobs()
		{
			if (!Enabled)
			{
				OnRaiseAgentLogEntry(20, LogCategory.Info, "Agent was disabled. Terminating job processing task.");
				return;
			}
			OnRaiseAgentLogEntry(20, LogCategory.Info, "Checking for active jobs in Schedule Agent Queue table");

			Job nextJob = jobService.GetNextQueueJob(base.GetResourceGroupIDs(), base.AgentID);

			if (nextJob == null)
			{
				_logger.LogDebug("No active job found in Schedule Agent Queue table");
			}
			while (nextJob != null)
			{
				string agentMessage = string.Format(START_PROCESSING_JOB_MESSAGE_TEMPLATE, nextJob.JobId, nextJob.WorkspaceID,
					nextJob.TaskType);
				OnRaiseAgentLogEntry(1, LogCategory.Info, agentMessage);
				LogOnStartJobProcessing(agentMessage, nextJob.JobId, nextJob.WorkspaceID, nextJob.TaskType);

				TaskResult taskResult = ExecuteTask(nextJob);

				FinalizeJob(nextJob, taskResult);

				nextJob = jobService.GetNextQueueJob(base.GetResourceGroupIDs(), base.AgentID);
			}
			if (base.ToBeRemoved)
			{
				jobService.UnlockJobs(base.AgentID);
			}
		}

		private TaskResult ExecuteTask(Job job)
		{
			TaskResult result = new TaskResult() {Status = TaskStatusEnum.Success, Exceptions = null};
			ITask task = null;
			try
			{
				OnRaiseJobLogEntry(job, JobLogState.Started);
				LogOnStartJobExecution(job);
				task = GetTask(job);
				if (task != null)
				{
					StartTask(job, task);

					OnRaiseJobLogEntry(job, JobLogState.Finished);
					string msg = string.Format(FINISHED_PROCESSING_JOB_MESSAGE_TEMPLATE, job.JobId, job.WorkspaceID, job.TaskType);
					OnRaiseAgentLogEntry(1, LogCategory.Info, msg);
					LogOnFinishJobExecution(job);
				}
				else
				{
					throw new Exception("Could not find corresponding Task.");
				}
			}
			catch (Exception ex)
			{
				result.Status = TaskStatusEnum.Fail;
				result.Exceptions = new List<Exception>() {ex};
				OnRaiseException(job, ex);
				OnRaiseJobLogEntry(job, JobLogState.Error, ex);
				LogOnJobExecutionError(job, ex);
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
				WorkspaceId = job.WorkspaceID,
				UserId = job.SubmittedBy,
				ActionName = task.GetType().Name
			};

			using (_logger.LogContextPushProperties(context))
			{
				task.Execute(job);
			}
		}

		private void FinalizeJob(Job job, TaskResult taskResult)
		{
			try
			{
				FinalizeJobResult result = jobService.FinalizeJob(job, this.ScheduleRuleFactory, taskResult);
				OnRaiseJobLogEntry(job, result.JobState, null, result.Details);
				LogOnFinalizeJob(job, result);
			}
			catch (Exception ex)
			{
				OnRaiseException(job, ex);
				OnRaiseJobLogEntry(job, JobLogState.Error, ex);
				LogOnFinalizeJobError(job, ex);
			}
		}

		private void CleanupQueueJobs()
		{
			OnRaiseAgentLogEntry(20, LogCategory.Info, "Cleanup jobs");
			LogOnCleanupJobs();

			jobService.CleanupJobQueueTable();
		}

		protected virtual void OnRaiseAgentLogEntry(int level, LogCategory category, string message,
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

			if (RaiseAgentLogEntry != null)
				RaiseAgentLogEntry(category, message, detailmessage);
		}

		protected virtual void OnRaiseJobLogEntry(Job job, JobLogState state, Exception exception = null,
			string details = null)
		{
			if (RaiseJobLogEntry != null)
			{
				if (exception != null)
				{
					if (!string.IsNullOrEmpty(details)) details += Environment.NewLine;
					details += exception.Message + Environment.NewLine + exception.StackTrace;
				}
				RaiseJobLogEntry(job, state, details);
			}
		}

		protected virtual void OnRaiseException(Job job, Exception ex)
		{
			if (RaiseException != null)
				RaiseException(job, ex);
		}

		#region Logging

		private void LogOnInitialize(string message)
		{
			_logger.LogInformation(message);
		}

		private void LogOnInitializeManagerConfigSettingsFactory()
		{
			_logger.LogInformation("Attempting to initialize Config Settings factory in {TypeName}",
				nameof(ScheduleQueueAgentBase));
		}

		private void LogOnCleanupJobs()
		{
			_logger.LogInformation("Attempting to cleanup jobs in {TypeName}", nameof(ScheduleQueueAgentBase));
		}

		private void LogOnExecuteCompleteWithErrors()
		{
			_logger.LogInformation("Execution of scheduled jobs completed with errors in {TypeName}",
				nameof(ScheduleQueueAgentBase));
		}

		private void LogOnExecuteComplete()
		{
			_logger.LogInformation("Execution of scheduled jobs completed in {TypeName}", nameof(ScheduleQueueAgentBase));
		}

		private void LogOnExecuteError(Exception ex)
		{
			_logger.LogError(ex, "An error occured during executing scheduled jobs in {TypeName}", nameof(ScheduleQueueAgentBase));
		}

		private void LogOnStartJobProcessing(string agentMessage, long nextJobJobId, int nextJobWorkspaceId,
			string nextJobTaskType)
		{
			_logger.LogInformation(agentMessage, nextJobJobId, nextJobWorkspaceId, nextJobTaskType);
		}

		private void LogOnStartJobExecution(Job job)
		{
			_logger.LogInformation("Attempting to execute Job with ID: {JobID} in {TypeName}", job.JobId,
				nameof(ScheduleQueueAgentBase));
		}

		private void LogOnFinishJobExecution(Job job)
		{
			_logger.LogInformation("Finished execution of Job with ID: {JobID} in {TypeName}", job.JobId,
				nameof(ScheduleQueueAgentBase));
		}

		private void LogOnJobExecutionError(Job job, Exception exception)
		{
			_logger.LogError(exception, "An error occured during execution of Job with ID: {JobID} in {TypeName}", job.JobId,
				nameof(ScheduleQueueAgentBase));
		}

		private void LogOnFinalizeJob(Job job, FinalizeJobResult result)
		{
			string message = $"Finished Finalization of Job with ID: {job.JobId} in {nameof(ScheduleQueueAgentBase)}." +
							$"{Environment.NewLine}Job result: {result.JobState}," +
							$"{Environment.NewLine} Details: {result.Details}";
				
			_logger.LogInformation(message);
		}

		private void LogOnFinalizeJobError(Job job, Exception exception)
		{
			_logger.LogError(exception, "An error occured during finalization of Job with ID: {JobID} in {TypeName}", job.JobId,
				nameof(ScheduleQueueAgentBase));
		}

		#endregion
	}
}