using System;
using System.Collections.Generic;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;

namespace kCura.ScheduleQueue.AgentBase
{
	public abstract class ScheduleQueueAgentBase : Agent.AgentBase
	{
		private IJobService _jobService;
		private const int _MAX_MESSAGE_LENGTH = 10000;
		private readonly Guid _agentGuid;

		protected IAPILog Logger { get; set; }

		public ScheduleQueueAgentBase(Guid agentGuid,
			IAgentService agentService = null, IJobService jobService = null,
			IScheduleRuleFactory scheduleRuleFactory = null)
		{
			_agentGuid = agentGuid;
			AgentService = agentService;
			_jobService = jobService;
			ScheduleRuleFactory = scheduleRuleFactory ?? new DefaultScheduleRuleFactory();
		}

		public IAgentService AgentService { get; private set; }
		public IScheduleRuleFactory ScheduleRuleFactory { get; private set; }

		protected virtual void Initialize()
		{
			//Logger cannot be initialized in constructor because Helper from Agent.Base is initialized later on
			Logger = Helper.GetLoggerFactory().GetLogger().ForContext<ScheduleQueueAgentBase>();

			NotifyAgentTab(20, LogCategory.Debug, "Initialize Agent core services");

			if (AgentService == null)
			{
				AgentService = new AgentService(Helper, _agentGuid);
			}

			if (_jobService == null)
			{
				_jobService = new JobService(AgentService, new JobServiceDataProvider(AgentService, Helper), Helper);
			}
		}

		public sealed override void Execute()
		{
			NotifyAgentTab(10, LogCategory.Info, "Started.");

			bool isPreExecuteSuccessful = PreExecute();
			if (isPreExecuteSuccessful)
			{
				ProcessQueueJobs();
				CleanupQueueJobs();
			}

			CompleteExecution();
		}
		
		private bool PreExecute()
		{
			try
			{
				Initialize();
				InitializeManagerConfigSettingsFactory();
				CheckQueueTable();
			}
			catch (Exception ex)
			{
				NotifyAgentTab(20, LogCategory.Warn, $"{ex.Message} {ex.StackTrace}");
				LogExecuteError(ex);
				return false;
			}
			return true;
		}

		private void CompleteExecution()
		{
			NotifyAgentTab(10, LogCategory.Info, "Completed.");
			LogExecuteComplete();
		}

		private void InitializeManagerConfigSettingsFactory()
		{
			NotifyAgentTab(20, LogCategory.Info, "Initialize Config Settings factory");
			LogOnInitializeManagerConfigSettingsFactory();
			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);
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
			try
			{
				Job nextJob = GetNextQueueJob();
				if (nextJob == null)
				{
					Logger.LogDebug("No active job found in Schedule Agent Queue table");
				}
				while (nextJob != null)
				{
					TaskResult jobResult = ProcessJob(nextJob);
					FinalizeJobExecution(nextJob, jobResult);
					nextJob = GetNextQueueJob(); // assumptions: it will not throws exception
				}

				if (ToBeRemoved)
				{
					_jobService.UnlockJobs(AgentID); // what if exception
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Unhandled exception occured while processing queue jobs.");
			}
		}

		protected abstract TaskResult ProcessJob(Job job);

		private void FinalizeJobExecution(Job job, TaskResult taskResult)
		{
			try
			{
				FinalizeJobResult result = _jobService.FinalizeJob(job, ScheduleRuleFactory, taskResult);
				LogJobState(job, result.JobState, null, result.Details);
				LogFinalizeJob(job, result);
			}
			catch (Exception ex)
			{
				LogFinalizeJobError(job, ex);
			}
		}

		private Job GetNextQueueJob()
		{
			if (!Enabled)
			{
				NotifyAgentTab(20, LogCategory.Info, "Agent was disabled. Terminating job processing task.");
				return null;
			}
			NotifyAgentTab(20, LogCategory.Info, "Checking for active jobs in Schedule Agent Queue table");

			try
			{
				return _jobService.GetNextQueueJob(GetListOfResourceGroupIDs(), AgentID);
			}
			catch (Exception ex)
			{
				LogGetNextQueueJobException(ex);
				NotifyAgentTab(20, LogCategory.Exception, "Exception while getting next job from queue.");
				return null;
			}
		}

		private void CleanupQueueJobs()
		{
			NotifyAgentTab(20, LogCategory.Info, "Cleanup jobs");
			LogOnCleanupJobs();

			try
			{
				_jobService.CleanupJobQueueTable();
			}
			catch (Exception ex)
			{
				NotifyAgentTab(20, LogCategory.Warn, $"An error occured cleaning jobs queue. {ex.Message} {ex.StackTrace}");
				LogCleanupError(ex);
			}
		}

		protected void NotifyAgentTab(int level, LogCategory category, string message,
			string detailmessage = null)
		{
			string msg = message.Substring(0, Math.Min(message.Length, _MAX_MESSAGE_LENGTH));
			switch (category)
			{
				case LogCategory.Info:
				case LogCategory.Debug:
					RaiseMessage(msg, level);
					break;
				case LogCategory.Exception:
					RaiseError(msg, detailmessage);
					break;
				case LogCategory.Warn:
					RaiseWarning(msg);
					break;
				default:
					throw new ArgumentOutOfRangeException("category");
			}
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

		private void LogCleanupError(Exception ex)
		{
			Logger.LogError(ex, "An error occured during cleaning schedule queue table in {TypeName}", nameof(ScheduleQueueAgentBase));
		}

		private void LogGetNextQueueJobException(Exception ex)
		{
			Logger.LogError(ex, "An error occured during getting next queue job");
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