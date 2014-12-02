using System;
using System.Collections.Generic;
using kCura.Agent;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils;
using kCura.ScheduleQueueAgent.Helpers;
using kCura.ScheduleQueueAgent.Services;
using Relativity.API;

namespace kCura.ScheduleQueueAgent
{
	public delegate void AgentLoggingEventHandler(LogCategory category, string message, string detailmessage);
	public delegate void JobLoggingEventHandler(Job job, JobLogState state, string details = null);
	public delegate void ExceptionEventHandler(Job job, Exception exception);

	public abstract class ScheduleQueueAgentBase : AgentBase, ITaskFactory
	{
		public event AgentLoggingEventHandler RaiseAgentLogEntry;
		public event JobLoggingEventHandler RaiseJobLogEntry;
		public event ExceptionEventHandler RaiseException;

		private IJobService jobService = null;
		private bool errorRaised = false;

		#region Constants
		private const string PROCESSING_JOB_MESSAGE_TEMPLATE = "Processing Job ID : {0} : Workspace ID {1} : Job Type {2}";
		private const string START_PROCESSING_JOB_MESSAGE_TEMPLATE = "Started : " + PROCESSING_JOB_MESSAGE_TEMPLATE;
		private const string FINISHED_PROCESSING_JOB_MESSAGE_TEMPLATE = "Finished : " + PROCESSING_JOB_MESSAGE_TEMPLATE;
		private const int MAX_MESSAGE_LENGTH = 10000;
		#endregion

		public ScheduleQueueAgentBase()
		{
			DBContext = base.Helper.GetDBContext(-1);
			Guid agentGuid = new QueueTableHelper().GetAgentGuid();
			this.AgentService = new AgentService(DBContext, agentGuid);
			this.jobService = new JobService(AgentService, DBContext);
		}

		//for testing
		public ScheduleQueueAgentBase(IDBContext dbContext, IAgentService agentService, IJobService jobService)
		{
			this.AgentService = agentService;
			this.jobService = jobService;
			this.DBContext = dbContext;
		}

		public IDBContext DBContext { get; private set; }
		public IAgentService AgentService { get; private set; }

		public virtual ITask GetTask(Job job)
		{
			ITask task = jobService.GetTask(job);
			return task;
		}

		public TimeProvider TimeProvider
		{
			get { return TimeProvider.Current; }
			set { TimeProvider.Current = value; }
		}

		public sealed override void Execute()
		{
			errorRaised = false;

			OnRaiseAgentLogEntry(10, LogCategory.Info, "Started.");

			OnRaiseAgentLogEntry(20, LogCategory.Info, "Initialize Manager Config settings factory");
			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(base.Helper);

			OnRaiseAgentLogEntry(20, LogCategory.Info, "Check for Queue Table");
			CheckQueueTable();

			OnRaiseAgentLogEntry(20, LogCategory.Info, "Process jobs");
			ProcessQueueJobs();

			if (!errorRaised)
			{
				OnRaiseAgentLogEntry(10, LogCategory.Info, "Completed.");
			}
		}

		private void CheckQueueTable()
		{
			AgentService.CreateQueueTable();
		}

		public void ProcessQueueJobs()
		{
			var msg = string.Empty;

			Job nextJob = jobService.GetNextQueueJob(base.GetResourceGroupIDs());
			while (nextJob != null)
			{
				msg = string.Format(START_PROCESSING_JOB_MESSAGE_TEMPLATE, nextJob.JobId, nextJob.WorkspaceID, nextJob.TaskType);
				OnRaiseAgentLogEntry(1, LogCategory.Info, msg);

				//TODO: 
				//if (!jobService.IsWorkspaceActive(nextJob.WorkspaceID))
				//{
				//	var warnMsg = string.Format("Deleting job {0} from workspace {1} due to inactive workspace", nextJob.JobId, nextJob.JobType.ToString());
				//	mainLogger.Log(warnMsg, warnMsg, LogCategory.Warn);
				//	jobService.DeleteJob(nextJob.JobId);
				//	nextJob = jobService.GetNextQueueJob(AgentInformation, base.GetResourceGroupIDs());
				//	continue;
				//}

				TaskResult taskResult = ExecuteTask(nextJob);

				FinalizeJob(nextJob, taskResult);

				nextJob = jobService.GetNextQueueJob(base.GetResourceGroupIDs());
			}

			if (base.ToBeRemoved)
			{
				jobService.UnlockJobs(base.AgentID);
			}
		}

		private TaskResult ExecuteTask(Job job)
		{
			TaskResult result = new TaskResult() { Status = TaskStatusEnum.Success, Exceptions = null };
			try
			{
				OnRaiseJobLogEntry(job, JobLogState.Started);
				ITask task = GetTask(job);
				if (task != null)
				{
					task.Execute(job);

					OnRaiseJobLogEntry(job, JobLogState.Finished);
					string msg = string.Format(FINISHED_PROCESSING_JOB_MESSAGE_TEMPLATE, job.JobId, job.WorkspaceID, job.TaskType);
					OnRaiseAgentLogEntry(1, LogCategory.Info, msg);
				}
				else
				{
					throw new Exception("Could not find corresponding Task.");
				}
			}
			catch (Exception ex)
			{
				result.Status = TaskStatusEnum.Fail;
				result.Exceptions = new List<Exception>() { ex };
				OnRaiseException(job, ex);
				OnRaiseJobLogEntry(job, JobLogState.Error, ex);
			}
			return result;
		}

		private void FinalizeJob(Job job, TaskResult taskResult)
		{
			try
			{
				FinalizeJobResult result = jobService.FinalizeJob(job, taskResult);
				OnRaiseJobLogEntry(job, result.JobState, null, result.Details);
			}
			catch (Exception ex)
			{
				OnRaiseException(job, ex);
				OnRaiseJobLogEntry(job, JobLogState.Error, ex);
			}
		}

		protected virtual void OnRaiseAgentLogEntry(int level, LogCategory category, string message, string detailmessage = null)
		{
			string msg = message.Substring(0, MAX_MESSAGE_LENGTH);
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

		protected virtual void OnRaiseJobLogEntry(Job job, JobLogState state, Exception exception = null, string details = null)
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
	}
}
