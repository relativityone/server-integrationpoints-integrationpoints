using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Interfaces;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
using kCura.ScheduleQueue.Core.Validation;
using Relativity.API;
using Relativity.DataMigration.MigrateFileshareAccess;

namespace kCura.ScheduleQueue.AgentBase
{
    public abstract class ScheduleQueueAgentBase : Agent.AgentBase
	{
		private IAgentService _agentService;
		private IQueueQueryManager _queryManager;
		private Lazy<IKubernetesMode> _kubernetesModeLazy;
		private IJobService _jobService;
		private IQueueJobValidator _queueJobValidator;
		private IDateTime _dateTime;
        private IFileShareAccessService _fileShareAccessService;
        private const int _MAX_MESSAGE_LENGTH = 10000;

		private readonly Guid _agentGuid;
		private readonly Lazy<int> _agentId;
		private readonly Lazy<IAPILog> _loggerLazy;

		protected Func<IEnumerable<int>> GetResourceGroupIDsFunc { get; set; }

		private bool IsKubernetesMode => _kubernetesModeLazy.Value.IsEnabled();

		private static readonly Dictionary<LogCategory, int> _logCategoryToLogLevelMapping = new Dictionary<LogCategory, int>
		{
			[LogCategory.Debug] = 20,
			[LogCategory.Info] = 10
		};

		protected virtual IAPILog Logger => _loggerLazy.Value;

		protected IJobService JobService => _jobService;

		protected ScheduleQueueAgentBase(Guid agentGuid, IKubernetesMode kubernetesMode = null, IAgentService agentService = null, IJobService jobService = null,
			IScheduleRuleFactory scheduleRuleFactory = null, IQueueJobValidator queueJobValidator = null, 
			IQueueQueryManager queryManager = null, IDateTime dateTime = null, IAPILog logger = null,
			IFileShareAccessService fileShareAccessService = null)
		{
			// Lazy init is required for things depending on Helper
			// Helper property in base class is assigned AFTER object construction
			_loggerLazy = logger != null
				? new Lazy<IAPILog>(() => logger)
				: new Lazy<IAPILog>(InitializeLogger);

			_kubernetesModeLazy = kubernetesMode != null 
				? new Lazy<IKubernetesMode>(() => kubernetesMode)
				: new Lazy<IKubernetesMode>(() => new KubernetesMode(Logger));


			_agentGuid = agentGuid;
			_agentService = agentService;
			_jobService = jobService;
			_queueJobValidator = queueJobValidator;
			_dateTime = dateTime;
            _fileShareAccessService = fileShareAccessService;
            _queryManager = queryManager;
			ScheduleRuleFactory = scheduleRuleFactory ?? new DefaultScheduleRuleFactory();

			_agentId = new Lazy<int>(GetAgentID);
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
				_jobService = new JobService(_agentService, new JobServiceDataProvider(_queryManager), _kubernetesModeLazy.Value, Helper);
			}

			if (_queueJobValidator == null)
			{
				_queueJobValidator = new QueueJobValidator(Helper);
			}

			if (_dateTime == null)
			{
				_dateTime = new DateTimeWrapper();
			}

			if(GetResourceGroupIDsFunc == null)
            {
				GetResourceGroupIDsFunc = () => GetResourceGroupIDs();
			}

			if(_fileShareAccessService == null)
            {
				IMigrateFileshareFactory migrateFileshareFactory = 
					new MigrateFileshareFactory(
						new FileShareSecretProvider(Helper),
						new FileshareLogger(Logger));

				_fileShareAccessService = new FileShareAccessService(Helper, migrateFileshareFactory);
            }
		}

		public sealed override void Execute()
		{
			using (Logger.LogContextPushProperty("AgentRunCorrelationId", Guid.NewGuid()))
            {
	            if (IsKubernetesMode && DidWork.HasValue && DidWork.Value)
	            {
		            Logger.LogInformation("Shutting down agent after single job in K8s mode");
		            DidWork = false;
		            return;
	            }
	            
				if (ToBeRemoved)
				{
					Logger.LogInformation("Agent is marked to be removed. Job will not be processed.");
					return;
				}

				NotifyAgentTab(LogCategory.Debug, "Started.");

				try
				{
					PreExecute();

					ProcessQueueJobs();

					CleanupQueueJobs();

					CompleteExecution();
				}
				catch (Exception ex)
				{
					Logger.LogError(ex, "Unhandled exception occurred while processing job from queue. Unlocking the job");
					_jobService.UnlockJobs(_agentId.Value);
					DidWork = false;
				}
			}
		}
		
		private void PreExecute()
		{
			Initialize();
			InitializeManagerConfigSettingsFactory();
			CheckQueueTable();
			MountBCPPath();
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

		private void MountBCPPath()
        {
            if (!IsKubernetesMode)
            {
                Logger.LogInformation("Integration Points Agent is not enabled on shared compute environment. Mount BCPPath is not needed.");
                return;
            }

            _fileShareAccessService.MountBcpPathAsync().GetAwaiter().GetResult();
		}

		protected virtual IEnumerable<int> GetListOfResourceGroupIDs() // this method was added for unit testing purpose
		{
			return IsKubernetesMode ? Array.Empty<int>() : GetResourceGroupIDsFunc();
		}

		private void ProcessQueueJobs()
		{
			try
			{
				Job nextJob = GetNextQueueJob();
				if (nextJob == null)
				{
					Logger.LogInformation("No active job found in Schedule Agent Queue table");
					DidWork = false;
					return;
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

						if (!IsKubernetesMode)
						{
							nextJob = GetNextQueueJob(); // assumptions: it will not throws exception
						}
						else
						{
							nextJob = null;
						}
					}
				}

				if (ToBeRemoved)
				{
					_jobService.UnlockJobs(_agentId.Value); // what if exception
				}

				DidWork = true;
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Unhandled exception occurred while processing queue jobs. Unlocking the job");
				_jobService.UnlockJobs(_agentId.Value);
				DidWork = false;
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
			Logger.LogInformation("Finalize JobExecution: {job}", job.ToString());
			FinalizeJobResult result = _jobService.FinalizeJob(job, ScheduleRuleFactory, taskResult);
			LogJobState(job, result.JobState, null, result.Details);
			LogFinalizeJob(job, result);
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

			if(IsKubernetesMode)
            {
				LogAllJobsInTheQueue();
			}

			return _jobService.GetNextQueueJob(GetListOfResourceGroupIDs(), _agentId.Value);
		}

		private void CleanupQueueJobs()
		{
			NotifyAgentTab(LogCategory.Debug, "Cleanup jobs");
			LogOnCleanupJobs();

			_jobService.CleanupJobQueueTable();
		}

		protected void NotifyAgentTab(LogCategory category, string message, string detailmessage = null)
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

		protected abstract void LogJobState(Job job, JobLogState state, Exception exception = null, string details = null);

		protected int GetAgentID()
		{
			if (IsKubernetesMode)
			{
				// we can omit some less relevant bits representing years (https://stackoverflow.com/a/2695525/1579824)
				return Math.Abs((int)(_dateTime.UtcNow.Ticks >> 23));
			}
			else
			{
				return AgentID;
			}
		}

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
			Logger.LogInformation("Job ID {jobId} has been picked up from the queue by Agent ID {agentId}. Job Information: {@job}", job.JobId, _agentId, job.RemoveSensitiveData());
		}

		private void LogAllJobsInTheQueue()
        {
			try
            {
				IEnumerable<Job> jobs = _jobService.GetAllScheduledJobs();

				Logger.LogInformation("Jobs in queue JobId-RootJobId-LockedByAgentId-StopState: {jobs}",
					string.Join(";", jobs?.Select(x => $"{x.JobId}-{x.RootJobId}-{x.LockedByAgentID}-{x.StopState}") ?? new List<string>()));
			}
			catch(Exception ex)
            {
				Logger.LogError(ex, "Unable to log jobs in queue.");
            }
        }

		#endregion
	}
}