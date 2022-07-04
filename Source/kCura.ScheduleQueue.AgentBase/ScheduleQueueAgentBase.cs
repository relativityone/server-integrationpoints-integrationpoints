using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Services;
using kCura.ScheduleQueue.Core.Validation;
using Relativity.API;

namespace kCura.ScheduleQueue.AgentBase
{
	public abstract class ScheduleQueueAgentBase : Agent.AgentBase
	{
		private IAgentService _agentService;
		private IQueueQueryManager _queueManager;
		private Lazy<IKubernetesMode> _kubernetesModeLazy;
		private IJobService _jobService;
		private IQueueJobValidator _queueJobValidator;
		private IDateTime _dateTime;
		private ITaskParameterHelper _taskParameterHelper;
		private IConfig _config;

		private const int _MAX_MESSAGE_LENGTH = 10000;

		private readonly Guid _agentGuid;
		private readonly Lazy<int> _agentId;
		private readonly Lazy<IAPILog> _loggerLazy;
		private readonly Guid _agentInstanceGuid;

		private readonly bool _shouldReadJobOnce = false; //Only for testing purposes. DO NOT MODIFY IT!

		public Guid AgentInstanceGuid => _agentInstanceGuid;

		protected Func<IEnumerable<int>> GetResourceGroupIDsFunc { get; set; }

		private bool IsKubernetesMode => _kubernetesModeLazy.Value.IsEnabled();

		private static readonly Dictionary<LogCategory, int> _logCategoryToLogLevelMapping = new Dictionary<LogCategory, int>
		{
			[LogCategory.Debug] = 20,
			[LogCategory.Info] = 10
		};

		private IDisposable _loggerAgentInstanceContext;

		protected virtual IAPILog Logger => _loggerLazy.Value;

		protected IJobService JobService => _jobService;

		protected ScheduleQueueAgentBase(Guid agentGuid, IKubernetesMode kubernetesMode = null, IAgentService agentService = null, IJobService jobService = null,
			IScheduleRuleFactory scheduleRuleFactory = null, IQueueJobValidator queueJobValidator = null,
			IQueueQueryManager queryManager = null, IDateTime dateTime = null, IAPILog logger = null, IConfig config = null)
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
			_queueManager = queryManager;
			_config = config;
			ScheduleRuleFactory = scheduleRuleFactory ?? new DefaultScheduleRuleFactory();

			_agentId = new Lazy<int>(GetAgentID);
			_agentInstanceGuid = Guid.NewGuid();
		}

		public IScheduleRuleFactory ScheduleRuleFactory { get; }

		protected virtual void Initialize()
		{
			NotifyAgentTab(LogCategory.Debug, "Initialize Agent core services");

			if (_queueManager == null)
			{
				_queueManager = new QueueQueryManager(Helper, _agentGuid);
			}

			if (_agentService == null)
			{
				_agentService = new AgentService(Helper, _queueManager, _agentGuid);
			}

			if (_jobService == null)
			{
				_jobService = new JobService(_agentService, new JobServiceDataProvider(_queueManager), _kubernetesModeLazy.Value, Helper);
			}

			if (_queueJobValidator == null)
			{
				_queueJobValidator = new QueueJobValidator(Helper, Logger);
			}

			if (_dateTime == null)
			{
				_dateTime = new DateTimeWrapper();
			}

			if (GetResourceGroupIDsFunc == null)
			{
				GetResourceGroupIDsFunc = () => GetResourceGroupIDs();
			}

			if (_taskParameterHelper == null)
            {
				_taskParameterHelper = new TaskParameterHelper(
					SerializerWithLogging.Create(Logger),
					new DefaultGuidService());
            }

			if(_config == null)
            {
				_config = IntegrationPoints.Config.Config.Instance;
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

				NotifyAgentTab(LogCategory.Debug, "Started.");

				try
				{
					PreExecute();

					CleanupInvalidJobs();

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

				if (IsKubernetesMode)
				{
					Logger.LogInformation("Shutting down agent after single job in K8s mode");
					DidWork = false;
				}
			}
		}

		private void CleanupInvalidJobs()
		{
            try
            {
				DateTime utcNow = _dateTime.UtcNow;
				TimeSpan transientStateJobTimeout = _config.TransientStateJobTimeout;

				Logger.LogInformation("Checking if jobs are in transient state: DateTimeUtc - {dateTimeNow}, TransientStateJobTimeout {transientStateJobTimeout}",
					utcNow, transientStateJobTimeout);

				IEnumerable<Job> transientStateJobs = _jobService.GetAllScheduledJobs();


					transientStateJobs = transientStateJobs.Where(x => (x.Heartbeat != null && utcNow.Subtract(x.Heartbeat.Value) > transientStateJobTimeout)
						|| (x.LockedByAgentID == null && x.StopState != StopState.None && x.StopState != StopState.DrainStopped));
				foreach (var job in transientStateJobs)
				{
					Logger.LogError("Job {jobId}, will be failed due timeout. " +
							"LockedByAgent: {lockedByAgent}, " +
							"StopState: {stopState}, " +
							"Last Hearbeat: {heartbeat}, " +
							"DateTimeUtc {utcNow}",
						job.JobId, job.LockedByAgentID, job.StopState, job.Heartbeat, utcNow);

					job.MarkJobAsFailed(new TimeoutException($"Job {job.JobId} has failed due timeout. Contact your system administrator."), false);
					TaskResult result = ProcessJob(job);
					FinalizeJobExecution(job, result);
				}
			}
            catch (Exception ex)
            {
				Logger.LogError(ex, "Error occurred during cleaning invalid jobs from the queue.");
            }
		}

        private void PreExecute()
		{
			Initialize();
			InitializeManagerConfigSettingsFactory();
			CheckQueueTable();
		}

		private void CompleteExecution()
		{
			NotifyAgentTab(LogCategory.Debug, "Completed.");
			LogExecuteComplete();
		}

		private void InitializeManagerConfigSettingsFactory()
		{
			NotifyAgentTab(LogCategory.Debug, "Initialize Config Settings factory");			
			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);
		}

		private void CheckQueueTable()
		{
			NotifyAgentTab(LogCategory.Debug, "Check Schedule Agent Queue table exists");

			_agentService.InstallQueueTable();
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

                TaskResult jobResult = new TaskResult() { Status = TaskStatusEnum.None };
                while (nextJob != null)
                {
                    AgentCorrelationContext context = GetCorrelationContext(nextJob);
                    using (Logger.LogContextPushProperties(context))
                    {
                        PreValidationResult validationResult = PreExecuteJobValidation(nextJob);

						if(!validationResult.ShouldExecute)
                        {
							Logger.LogInformation("Job {jobId} is not valid. It will be removed from the queue.", nextJob.JobId);
							
							TaskResult failedJobResult = new TaskResult
							{ 
								Status = TaskStatusEnum.Fail,
								Exceptions = new List<Exception> { validationResult.Exception }
							};

							nextJob.MarkJobAsFailed(validationResult.Exception, true);
							FinalizeJobExecution(nextJob, failedJobResult);
							
							nextJob = GetNextQueueJob();
							continue;
						}

                        Logger.LogInformation("Starting Job {jobId} processing...", nextJob.JobId);

                        jobResult = ProcessJob(nextJob);

                        if (jobResult.Status == TaskStatusEnum.DrainStopped)
                        {
                            Logger.LogInformation(
                                "Job {jobId} has been drain-stopped. No other jobs will be picked up.", nextJob.JobId);
                            _jobService.FinalizeDrainStoppedJob(nextJob);
                            break;
                        }

                        Logger.LogInformation("Job {jobId} has been processed with status {status}", nextJob.JobId,
                            jobResult.Status.ToString());
                        FinalizeJobExecution(nextJob, jobResult);
                    }

                    if (!IsKubernetesMode)
                    {
                        nextJob = GetNextQueueJob(); // assumptions: it will not throw exception
                    }
                    else
                    {
						nextJob = GetNextQueueJob(nextJob.RootJobId);
						if (nextJob == null)
                        {
							break;
						}

						Logger.LogInformation("Pick up another Job {jobId} with corresponding RootJobId - {rootJobId}", nextJob.JobId, nextJob.RootJobId);
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

		private AgentCorrelationContext GetCorrelationContext(Job job)
        {
			Guid batchInstanceId = _taskParameterHelper.GetBatchInstance(job);
			string correlationId = batchInstanceId.ToString();

			return new AgentCorrelationContext
			{
				JobId = job.JobId,
				RootJobId = job.RootJobId,
				WorkspaceId = job.WorkspaceID,
				UserId = job.SubmittedBy,
				IntegrationPointId = job.RelatedObjectArtifactID,
				WorkflowId = correlationId,
				ActionName = job.TaskType
			};
		}

		private PreValidationResult PreExecuteJobValidation(Job job)
        {
            try
            {
                PreValidationResult result = _queueJobValidator.ValidateAsync(job).GetAwaiter().GetResult();
                if (!result.IsValid)
                {
					job.MarkJobAsFailed(result.Exception, result.ShouldBreakSchedule);
                    LogValidationJobFailed(job, result);
                }

                return result;
            }
            catch (Exception e)
			{
				Logger.LogError(e, "Error occurred during Queue Job Validation. Return job as valid and try to run.");
				return PreValidationResult.Success;
			}

		}

		protected abstract TaskResult ProcessJob(Job job);

		private void FinalizeJobExecution(Job job, TaskResult taskResult)
		{
			Logger.LogInformation("Finalize JobExecution with result: {result}, Job: {job}", taskResult.Status, job.ToString());
			
			FinalizeJobResult result = _jobService.FinalizeJob(job, ScheduleRuleFactory, taskResult);

			Exception exception = taskResult.Exceptions.Any() ? new AggregateException(taskResult.Exceptions) : null;
			LogJobState(job, result.JobState, exception, result.Details);
			
			LogFinalizeJob(job, result);
		}

		private Job GetNextQueueJob(long? rootJobId = null)
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

			if(_shouldReadJobOnce)
            {
				Logger.LogWarning("This line should not be reached in production! ShouldReadJobOnce - {shouldReadJobOnce}", _shouldReadJobOnce);
				return null;
            }

			return _jobService.GetNextQueueJob(GetListOfResourceGroupIDs(), _agentId.Value, rootJobId);
		}

		private void CleanupQueueJobs()
		{
			NotifyAgentTab(LogCategory.Debug, "Cleanup jobs");
			_jobService.CleanupJobQueueTable();
		}

		protected void NotifyAgentTab(LogCategory category, string message, string detailmessage = null)
		{
			string msg = message.Substring(0, Math.Min(message.Length, _MAX_MESSAGE_LENGTH));
			switch (category)
			{
				case LogCategory.Debug:
					RaiseMessageNoLogging(msg, _logCategoryToLogLevelMapping[LogCategory.Debug]);					
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
			
			IAPILog logger = Helper.GetLoggerFactory().GetLogger().ForContext<ScheduleQueueAgentBase>();
			_loggerAgentInstanceContext = logger.LogContextPushProperty("AgentInstanceGuid", AgentInstanceGuid);
			
			return logger;
		}

		#region Logging		

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
			string message = $"Finished Finalization of Job with ID: {job.JobId} in {nameof(ScheduleQueueAgentBase)}." + $"{Environment.NewLine}Job result: {result.JobState}";

			Logger.LogInformation(message);
		}

		private void LogFinalizeJobError(Job job, Exception exception)
		{
			Logger.LogError(exception, "An error occured during finalization of Job with ID: {JobID} in {TypeName}", job.JobId,
				nameof(ScheduleQueueAgentBase));
		}

		private void LogValidationJobFailed(Job job, PreValidationResult result)
		{
			Logger.LogInformation("Job {jobId} validation failed with message: {message}", job.JobId, result.Exception?.Message);
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