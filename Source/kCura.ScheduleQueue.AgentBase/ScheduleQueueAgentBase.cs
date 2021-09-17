using System;
using System.Collections.Generic;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
using kCura.ScheduleQueue.Core.Validation;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.ScheduleQueue.AgentBase
{
	public abstract class ScheduleQueueAgentBase : Agent.AgentBase
	{
		private IAgentService _agentService;
		private IQueueQueryManager _queryManager;
		private IJobService _jobService;
		private IQueueJobValidator _queueJobValidator;
		private const int _MAX_MESSAGE_LENGTH = 10000;
		private readonly Guid _agentGuid;
		private readonly Lazy<IAPILog> _loggerLazy;
		private static readonly Dictionary<LogCategory, int> _logCategoryToLogLevelMapping = new Dictionary<LogCategory, int>
		{
			[LogCategory.Debug] = 20,
			[LogCategory.Info] = 10
		};

		protected IAPILog Logger => _loggerLazy.Value;

		protected IJobService JobService => _jobService;

		protected ScheduleQueueAgentBase(Guid agentGuid, IAgentService agentService = null, IJobService jobService = null,
			IScheduleRuleFactory scheduleRuleFactory = null, IQueueJobValidator queueJobValidator = null, 
			IQueueQueryManager queryManager = null, IAPILog logger = null)
		{
			_agentGuid = agentGuid;
			_agentService = agentService;
			_jobService = jobService;
			_queueJobValidator = queueJobValidator;
			_queryManager = queryManager;
			ScheduleRuleFactory = scheduleRuleFactory ?? new DefaultScheduleRuleFactory();

			_loggerLazy = logger != null
				? new Lazy<IAPILog>(() => logger)
				: new Lazy<IAPILog>(InitializeLogger);
		}
		public IScheduleRuleFactory ScheduleRuleFactory { get; }

		protected virtual void Initialize()
		{
			NotifyAgentTab(LogCategory.Debug, "Initialize Agent core services");


			if (_queryManager == null)
			{
				_queryManager = new QueueQueryManager(Helper, _agentGuid);
			}

			if (_agentService == null)
			{
				_agentService = new AgentService(Helper, _queryManager, _agentGuid);
			}

			if (_jobService == null)
			{
				_jobService = new JobService(_agentService, new JobServiceDataProvider(_queryManager), ToggleProvider.Current, Helper);
			}

			if (_queueJobValidator == null)
			{
				_queueJobValidator = new QueueJobValidator(Helper);
			}
		}

		public sealed override void Execute()
		{
			using (Logger.LogContextPushProperty("AgentRunCorrelationId", Guid.NewGuid()))
            {
				if (ToBeRemoved)
				{
					Logger.LogInformation("Agent is marked to be removed. Job will not be processed.");
					return;
				}
				bool isPreExecuteSuccessful = PreExecute();

				NotifyAgentTab(LogCategory.Debug, "Started.");

				if (isPreExecuteSuccessful)
				{
					ProcessQueueJobs();
					CleanupQueueJobs();
				}

				CompleteExecution();

				DidWork = true;
			}
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
				NotifyAgentTab(LogCategory.Warn, $"{ex.Message} {ex.StackTrace}");
				LogExecuteError(ex);
				return false;
			}
			return true;
		}

		private void CompleteExecution()
		{
			NotifyAgentTab(LogCategory.Debug, "Completed.");
			LogExecuteComplete();
		}

		private void InitializeManagerConfigSettingsFactory()
		{
			NotifyAgentTab(LogCategory.Debug, "Initialize Config Settings factory");
			LogOnInitializeManagerConfigSettingsFactory();
			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);
		}

		private void CheckQueueTable()
		{
			NotifyAgentTab(LogCategory.Debug, "Check Schedule Agent Queue table exists");

			_agentService.InstallQueueTable();
		}

		protected virtual IEnumerable<int> GetListOfResourceGroupIDs() // this method was added for unit testing purpose
		{
			return GetResourceGroupIDs();
		}

		private void ProcessQueueJobs()
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
					LogJobInformation(nextJob);

					bool isJobValid = PreExecuteJobValidation(nextJob);
					if (!isJobValid)
					{
						Logger.LogInformation("Deleting invalid Job {jobId}...", nextJob.JobId);

						_jobService.DeleteJob(nextJob.JobId);
						nextJob = GetNextQueueJob();
						continue;
					}

					Logger.LogInformation("Starting Job {jobId} processing...", nextJob.JobId);

					TaskResult jobResult = ProcessJob(nextJob);

					Logger.LogInformation("Job {jobId} has been processed with status {status}", nextJob.JobId, jobResult.Status.ToString());
					
					// If last job was drain-stopped, assign null to nextJob so it doesn't get executed on next loop iteration.
					// Also do not finalize the job (i.e. do not remove it from the queue).
					if (jobResult.Status == TaskStatusEnum.DrainStopped)
					{
						Logger.LogInformation("Job has been drain-stopped. No other jobs will be picked up.");
						_jobService.FinalizeDrainStoppedJob(nextJob);
                        nextJob = null;
					}
					else
					{
						FinalizeJobExecution(nextJob, jobResult);
						nextJob = GetNextQueueJob(); // assumptions: it will not throws exception
					}
				}

				if (ToBeRemoved)
				{
					_jobService.UnlockJobs(AgentID); // what if exception
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Unhandled exception occurred while processing queue jobs.");
			}
		}

		private bool PreExecuteJobValidation(Job job)
		{
			try
			{
				ValidationResult result = _queueJobValidator.ValidateAsync(job).GetAwaiter().GetResult();
				if (!result.IsValid)
				{
					LogValidationJobFailed(job, result);
				}

				return result.IsValid;
			}
			catch (Exception e)
			{
				Logger.LogError(e, "Error occurred during Queue Job Validation. Return job as valid and try to run.");
				return true;
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
				NotifyAgentTab(LogCategory.Info, "Agent was disabled. Terminating job processing task.");
				return null;
			}

			if (ToBeRemoved)
			{
				NotifyAgentTab(LogCategory.Info, "Agent is going to be removed. Cannot process any more jobs.");
				return null;
			}

			NotifyAgentTab(LogCategory.Debug, "Checking for active jobs in Schedule Agent Queue table");

			try
			{
				return _jobService.GetNextQueueJob(GetListOfResourceGroupIDs(), AgentID);
			}
			catch (Exception ex)
			{
				LogGetNextQueueJobException(ex);
				NotifyAgentTab(LogCategory.Exception, "Exception while getting next job from queue.");
				return null;
			}
		}

		private void CleanupQueueJobs()
		{
			NotifyAgentTab(LogCategory.Debug, "Cleanup jobs");
			LogOnCleanupJobs();

			try
			{
				_jobService.CleanupJobQueueTable();
			}
			catch (Exception ex)
			{
				NotifyAgentTab(LogCategory.Warn, $"An error occurred cleaning jobs queue. {ex.Message} {ex.StackTrace}");
				LogCleanupError(ex);
			}
		}

		protected void NotifyAgentTab(LogCategory category, string message,
			string detailmessage = null)
		{
			string msg = message.Substring(0, Math.Min(message.Length, _MAX_MESSAGE_LENGTH));
			switch (category)
			{
				case LogCategory.Debug:
					RaiseMessageNoLogging(msg, _logCategoryToLogLevelMapping[LogCategory.Debug]);
					Logger.LogDebug(message);
					break;
				case LogCategory.Info:
					RaiseMessage(msg, _logCategoryToLogLevelMapping[LogCategory.Info]);
					break;
				case LogCategory.Warn:
					RaiseWarning(msg);
					break;
				case LogCategory.Exception:
					RaiseError(msg, detailmessage);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(category));
			}
		}

		protected abstract void LogJobState(Job job, JobLogState state, Exception exception = null,
			string details = null);

		private IAPILog InitializeLogger()
		{
			if (Helper == null)
			{
				NotifyAgentTab(LogCategory.Exception, "Logger initialization failed. Helper is null.");
			}

			return Helper.GetLoggerFactory().GetLogger().ForContext<ScheduleQueueAgentBase>();
		}

		#region Logging

		private void LogOnInitializeManagerConfigSettingsFactory()
		{
			Logger.LogDebug("Attempting to initialize Config Settings factory in {TypeName}",
				nameof(ScheduleQueueAgentBase));
		}

		private void LogOnCleanupJobs()
		{
			Logger.LogDebug("Attempting to cleanup jobs in {TypeName}", nameof(ScheduleQueueAgentBase));
		}

		private void LogExecuteComplete()
		{
			Logger.LogDebug("Execution of scheduled jobs completed in {TypeName}", nameof(ScheduleQueueAgentBase));
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
			string message = $"Finished Finalization of Job with ID: {job.JobId} in {nameof(ScheduleQueueAgentBase)}." + $"{Environment.NewLine}Job result: {result.JobState}";

			Logger.LogInformation(message);
		}

		private void LogFinalizeJobError(Job job, Exception exception)
		{
			Logger.LogError(exception, "An error occured during finalization of Job with ID: {JobID} in {TypeName}", job.JobId,
				nameof(ScheduleQueueAgentBase));
		}

		private void LogValidationJobFailed(Job job, ValidationResult result)
		{
			Logger.LogInformation("Job {jobId} validation failed with message: {message}", job.JobId, result.Message);
		}

		private void LogJobInformation(Job job)
		{
			Logger.LogInformation("Job {jobId} has been picked up from the queue. Job Information: " +
			                    "WorkspaceID: {workspaceId} " +
			                    "RelatedObjectArtifactID: {relatedObjectArtifactId} " +
			                    "Task types: {taskType} " +
			                    "Submitted by: {submittedBy} " +
								"Details: {jobDetails}" +
								"StopState: {stopState}",
					job.JobId, job.WorkspaceID, job.RelatedObjectArtifactID, job.TaskType, job.SubmittedBy, job.JobDetails, job.StopState);
		}
		#endregion
	}
}