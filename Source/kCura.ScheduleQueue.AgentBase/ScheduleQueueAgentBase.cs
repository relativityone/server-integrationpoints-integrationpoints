using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Checkers;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter.DNS;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Validation;
using Relativity.API;
using Relativity.Telemetry.APM;

namespace kCura.ScheduleQueue.AgentBase
{
    public abstract class ScheduleQueueAgentBase : Agent.AgentBase
    {
        private readonly Guid _agentGuid;
        private readonly Lazy<int> _agentId;
        private readonly Lazy<IAPILog> _loggerLazy;
        private readonly Guid _agentInstanceGuid;
        private readonly bool _shouldReadJobOnce = false; // Only for testing purposes. DO NOT MODIFY IT!

        private bool IsKubernetesMode => _kubernetesModeLazy.Value.IsEnabled();

        private IAgentService _agentService;
        private IQueueQueryManager _queueManager;
        private Lazy<IKubernetesMode> _kubernetesModeLazy;
        private IJobService _jobService;
        private IQueueJobValidator _queueJobValidator;
        private IDateTime _dateTime;
        private ITaskParameterHelper _taskParameterHelper;
        private IConfig _config;
        private IAPM _apm;
        private IDbContextFactory _dbContextFactory;
        private IRelativityObjectManagerFactory _objectManagerFactory;
        private DateTime _agentStartTime;

        protected ScheduleQueueAgentBase(
            Guid agentGuid,
            IKubernetesMode kubernetesMode = null,
            IAgentService agentService = null,
            IJobService jobService = null,
            IScheduleRuleFactory scheduleRuleFactory = null,
            IQueueJobValidator queueJobValidator = null,
            IQueueQueryManager queryManager = null,
            IDateTime dateTime = null,
            IAPILog logger = null,
            IConfig config = null,
            IAPM apm = null,
            IDbContextFactory dbContextFactory = null,
            IRelativityObjectManagerFactory objectManagerFactory = null)
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
            _apm = apm;
            _dbContextFactory = dbContextFactory;
            ScheduleRuleFactory = scheduleRuleFactory ?? new DefaultScheduleRuleFactory();

            _objectManagerFactory = objectManagerFactory;

            _agentId = new Lazy<int>(GetAgentID);
            _agentInstanceGuid = Guid.NewGuid();
        }

        public Guid AgentInstanceGuid => _agentInstanceGuid;

        public IScheduleRuleFactory ScheduleRuleFactory { get; }

        protected Func<IEnumerable<int>> GetResourceGroupIDsFunc { get; set; }

        protected virtual IAPILog Logger => _loggerLazy.Value;

        protected IJobService JobService => _jobService;

        public sealed override void Execute()
        {
            using (Logger.LogContextPushProperty("AgentRunCorrelationId", Guid.NewGuid()))
            {
                if (ToBeRemoved)
                {
                    Logger.LogInformation("Agent is marked to be removed. Job will not be processed.");
                    return;
                }

                Logger.LogInformation("Integration Points Agent execution started...");

                try
                {
                    PreExecute();

                    CleanupInvalidJobs();

                    ProcessQueueJobs();

                    CompleteExecution();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Unhandled exception occurred while processing job from queue. Unlocking the job");
                    DidWork = false;
                }

                if (IsKubernetesMode)
                {
                    Logger.LogInformation("Shutting down agent after single job in K8s mode");
                    DidWork = false;
                }
            }
        }

        protected virtual void Initialize()
        {
            Logger.LogInformation("Initialize Agent core services");

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

            if (_dateTime == null)
            {
                _dateTime = new DateTimeWrapper();
            }

            if (GetResourceGroupIDsFunc == null)
            {
                GetResourceGroupIDsFunc = GetResourceGroupIDs;
            }

            if (_taskParameterHelper == null)
            {
                _taskParameterHelper = new TaskParameterHelper(
                    new RipJsonSerializer(Logger),
                    new DefaultGuidService());
            }

            if (_config == null)
            {
                _config = IntegrationPoints.Config.Config.Instance;
            }

            if (_apm == null)
            {
                _apm = Client.APMClient;
            }

            if (_dbContextFactory == null)
            {
                _dbContextFactory = new DbContextFactory(Helper, Logger);
            }

            if (_objectManagerFactory == null)
            {
                _objectManagerFactory = new RelativityObjectManagerFactory(Helper);
            }

            if (_queueJobValidator == null)
            {
                _queueJobValidator = new QueueJobValidator(_objectManagerFactory, _config, ScheduleRuleFactory, Logger);
            }

            _agentStartTime = _dateTime.UtcNow;
        }

        protected virtual IEnumerable<int> GetListOfResourceGroupIDs() // this method was added for unit testing purpose
        {
            return IsKubernetesMode ? Array.Empty<int>() : GetResourceGroupIDsFunc();
        }

        protected abstract TaskResult ProcessJob(Job job);

        protected abstract void LogJobState(Job job, JobLogState state, Exception exception = null, string details = null);

        protected abstract void SendEmailNotificationForCrashedJob(Job job, IRelativityObjectManager objectManager, IntegrationPoint integrationPoint);

        protected int GetAgentID()
        {
            if (IsKubernetesMode)
            {
                // we can omit some less relevant bits representing years (https://stackoverflow.com/a/2695525/1579824)
                return Math.Abs((int)(_dateTime.UtcNow.Ticks >> 23));
            }

            return AgentID;
        }

        private void CleanupInvalidJobs()
        {
            try
            {
                DateTime utcNow = _dateTime.UtcNow;
                TimeSpan transientStateJobTimeout = _config.TransientStateJobTimeout;

                Logger.LogInformation(
                    "Checking if jobs are in transient state: DateTimeUtc - {dateTimeNow}, TransientStateJobTimeout {transientStateJobTimeout}",
                    utcNow,
                    transientStateJobTimeout);

                IEnumerable<Job> transientStateJobs = _jobService.GetAllScheduledJobs()
                    .Where(x => (x.Heartbeat != null && utcNow.Subtract(x.Heartbeat.Value) > transientStateJobTimeout) || x.IsBlocked())
                    .ToList();

                foreach (Job job in transientStateJobs)
                {
                    Logger.ForContext("TransientJob", job.RemoveSensitiveData(), true).LogInformation("Handling Transient Job {jobId}", job.JobId);

                    IRelativityObjectManager objectManager = _objectManagerFactory.CreateRelativityObjectManager(job.WorkspaceID);
                    IntegrationPoint integrationPoint = objectManager.Read<IntegrationPoint>(job.RelatedObjectArtifactID);

                    (SourceProvider sourceProvider, DestinationProvider destinationProvider) = GetProviders(job, objectManager, integrationPoint);

                    if (IsAzureADWorker(job, Guid.Parse(sourceProvider.ApplicationIdentifier)))
                    {
                        Logger.LogInformation("Job {jobId} is in unknown status. Because it's Azure AD Worker we'll unlock the job and pick it up agin.", job.JobId);
                        _jobService.UnlockJob(job);
                        continue;
                    }

                    Logger.LogError("Job {jobId} failed at {time} because Kubernetes Agent container crashed and job was left in unknown status. Job details: {@job}", job.JobId, utcNow, job.RemoveSensitiveData());

                    SendMetricAboutJobInTransientState(job, sourceProvider, destinationProvider);

                    PreValidationResult validationResult = PreExecuteJobValidation(job);
                    if (!validationResult.ShouldExecute)
                    {
                        RemoveInvalidJobFromQueue(validationResult, job);
                        continue;
                    }

                    job.MarkJobAsFailed(KubernetesException.CreateTransientJobException(job), false, false);
                    Logger.LogInformation("Starting Job in Transient State {jobId} processing...", job.JobId);

                    TaskResult result = ProcessJob(job);
                    FinalizeJobExecution(job, result);

                    SendEmailNotificationForCrashedJob(job, objectManager, integrationPoint);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred during cleaning invalid jobs from the queue.");
            }
        }

        private (SourceProvider, DestinationProvider) GetProviders(Job job, IRelativityObjectManager objectManager, IntegrationPoint integrationPoint)
        {
            Logger.LogInformation("SourceProvider was read from IntegrationPoint {integrationPointId} - SourceProviderId: {sourceProviderId}", job.RelatedObjectArtifactID, integrationPoint.SourceProvider);

            if (!integrationPoint.SourceProvider.HasValue)
            {
                const string message = "Source Provider does not have value assigned";
                Logger.LogError(message);
                throw new ApplicationException(message);
            }

            if (!integrationPoint.DestinationProvider.HasValue)
            {
                const string message = "Destination Provider does not have value assigned";
                Logger.LogError(message);
                throw new ApplicationException(message);
            }

            SourceProvider sourceProvider = objectManager.Read<SourceProvider>(integrationPoint.SourceProvider.Value);
            DestinationProvider destinationProvider = objectManager.Read<DestinationProvider>(integrationPoint.DestinationProvider.Value);

            return (sourceProvider, destinationProvider);
        }

        private bool IsAzureADWorker(Job job, Guid sourceProviderApplicationIdentifier)
        {
            try
            {
                Logger.LogInformation("Checking if Job {jobId} is AzureAD Type...", job.JobId);

                if (job.TaskType != nameof(TaskType.SyncWorker) && job.TaskType != nameof(TaskType.SyncEntityManagerWorker))
                {
                    return false;
                }

                Logger.LogInformation("Checking if {taskType} job is typeof AzureAD - {applicationIdentifier}", job.TaskType, sourceProviderApplicationIdentifier);

                return sourceProviderApplicationIdentifier == new Guid("8C8D2241-706A-47E1-B0C1-DB3F4F990DC5");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while checking if the Job {jobId} is type of Azure AD. JobDetails: {@job}", job.JobId, job);
                return false;
            }
        }

        private void SendMetricAboutJobInTransientState(Job job, SourceProvider sourceProvider, DestinationProvider destinationProvider)
        {
            Dictionary<string, object> jobInTransientStateCustomData = new Dictionary<string, object>()
            {
                { "r1.team.id", "PTCI-2456712" },
                { "service.name", "integrationpoints-repo" },
                { "JobId", job.JobId.ToString() },
                { "RootJobId", job.RootJobId.ToString() },
                { "LockedByAgentId", job.LockedByAgentID.ToString() },
                { "StopState", job.StopState.ToString() },
                { "LastHeartbeat", job.Heartbeat.ToString() }
            };

            ProviderType providerType = ProviderHelpers.GetProviderType(sourceProvider.Identifier, destinationProvider.Identifier);

            _apm.CountOperation($"IntegrationPoints.Performance.JobFailedCount.{providerType.ToString()}", customData: jobInTransientStateCustomData)
                .Write();
        }

        private void RemoveInvalidJobFromQueue(PreValidationResult validationResult, Job job)
        {
            Logger.LogInformation("Job {jobId} is not valid. It will be removed from the queue.", job.JobId);

            TaskResult failedJobResult = new TaskResult
            {
                Status = TaskStatusEnum.Fail,
                Exceptions = new List<Exception> { validationResult.Exception }
            };

            job.MarkJobAsFailed(validationResult.Exception, validationResult.ShouldBreakSchedule, validationResult.MaximumConsecutiveFailuresReached);
            FinalizeJobExecution(job, failedJobResult);
        }

        private void PreExecute()
        {
            Initialize();
            CheckServicesAccess();
            InitializeManagerConfigSettingsFactory();
            CheckQueueTable();
        }

        private void CheckServicesAccess()
        {
            IEddsDBContext eddsDBContext = _dbContextFactory.CreatedEDDSDbContext();
            IServiceHealthChecker dbHealthChecker = new DatabasePingReporter(eddsDBContext, Logger);
            IServiceHealthChecker keplerHealthChecker = new KeplerPingReporter(Helper, Logger);
            IServiceHealthChecker dnsHealthChecker = new DnsHealthReporter(new RealDnsService(), Logger);

            ServicesAccessChecker servicesAccessChecker = new ServicesAccessChecker(new[] { dbHealthChecker, keplerHealthChecker, dnsHealthChecker }, Logger);

            bool areServicesHealthy = servicesAccessChecker.AreServicesHealthyAsync().GetAwaiter().GetResult();

            if (!areServicesHealthy)
            {
                Logger.LogFatal("Not all Services are accessible by the Agent; _agentInstanceGuid - {_agentInstanceGuid}", _agentInstanceGuid);
                if (_kubernetesModeLazy.Value.IsEnabled())
                {
                    Environment.Exit(1);
                }

                throw new Exception($"Not all Services are accessible by the Agent; _agentInstanceGuid - {_agentInstanceGuid}");
            }
        }

        private void CompleteExecution()
        {
            LogExecuteComplete();
        }

        private void InitializeManagerConfigSettingsFactory()
        {
            Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);
        }

        private void CheckQueueTable()
        {
            _agentService.InstallQueueTable();
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
                        if (!validationResult.ShouldExecute)
                        {
                            RemoveInvalidJobFromQueue(validationResult, nextJob);
                            nextJob = GetNextQueueJob();
                            continue;
                        }

                        Logger.LogInformation("Starting Job {jobId} processing...", nextJob.JobId);

                        jobResult = ProcessJob(nextJob);

                        if (jobResult.Status == TaskStatusEnum.DrainStopped)
                        {
                            Logger.LogInformation(
                                "Job {jobId} has been drain-stopped. No other jobs will be picked up.",
                                nextJob.JobId);
                            _jobService.FinalizeDrainStoppedJob(nextJob);
                            break;
                        }

                        Logger.LogInformation(
                            "Job {jobId} has been processed with status {status}", // TODO REL-815726
                            nextJob.JobId,
                            jobResult.Status.ToString());
                        FinalizeJobExecution(nextJob, jobResult);
                    }

                    if (!IsKubernetesMode)
                    {
                        nextJob = GetNextQueueJob(); // assumptions: it will not throw exception
                    }
                    else
                    {
                        DateTime utcNow = _dateTime.UtcNow;
                        TimeSpan agentMaximumLifetime = _config.AgentMaximumLifetime;
                        if (utcNow.Subtract(_agentStartTime) > agentMaximumLifetime)
                        {
                            Logger.LogInformation(
                               "Integration Points Agent reached maximum lifetime value. Agent execution will be finished: " +
                               "UTCNow - {utcNow}, AgentStartTime - {agentStartTime}, AgentMaximumLifetime - {agentMaximumLifetime}",
                               utcNow,
                               _agentStartTime,
                               agentMaximumLifetime);

                            break;
                        }

                        nextJob = GetNextQueueJob(nextJob.RootJobId);
                        if (nextJob == null)
                        {
                            break;
                        }

                        Logger.LogInformation("Pick up another Job {jobId} with corresponding RootJobId - {rootJobId}", nextJob.JobId, nextJob.RootJobId);
                    }
                }

                DidWork = false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unhandled exception occurred while processing queue jobs. Unlocking the job");
                DidWork = false;
            }
        }

        private AgentCorrelationContext GetCorrelationContext(Job job)
        {
            return new AgentCorrelationContext
            {
                JobId = job.JobId,
                RootJobId = job.RootJobId,
                WorkspaceId = job.WorkspaceID,
                UserId = job.SubmittedBy,
                IntegrationPointId = job.RelatedObjectArtifactID,
                CorrelationId = job.CorrelationID,
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
                    job.MarkJobAsFailed(result.Exception, result.ShouldBreakSchedule, result.MaximumConsecutiveFailuresReached);
                    LogValidationJobFailed(job, result);
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred during Queue Job Validation. Return job as valid and try to run.");
                return PreValidationResult.InvalidJob(e.Message, false, false, false);
            }
        }

        private void FinalizeJobExecution(Job job, TaskResult taskResult)
        {
            Logger.LogInformation("Finalize JobExecution with result: {result}, Job: {@job}", taskResult.Status, job.RemoveSensitiveData());

            FinalizeJobResult result = _jobService.FinalizeJob(job, ScheduleRuleFactory, taskResult);

            Exception exception = taskResult.Exceptions != null && taskResult.Exceptions.Any() ? new AggregateException(taskResult.Exceptions) : null;
            LogJobState(job, result.JobState, exception, result.Details);

            LogFinalizeJob(job, result);
        }

        private Job GetNextQueueJob(long? rootJobId = null)
        {
            if (!Enabled)
            {
                Logger.LogInformation("Agent was disabled. Terminating job processing task.");
                return null;
            }

            if (ToBeRemoved)
            {
                Logger.LogInformation("Agent is going to be removed. Cannot process any more jobs.");
                return null;
            }

            if (IsKubernetesMode)
            {
                LogAllJobsInTheQueue();
            }

            if (_shouldReadJobOnce)
            {
                Logger.LogWarning("This line should not be reached in production! ShouldReadJobOnce - {shouldReadJobOnce}", _shouldReadJobOnce);
                return null;
            }

            return _jobService.GetNextQueueJob(GetListOfResourceGroupIDs(), _agentId.Value, rootJobId);
        }

        private IAPILog InitializeLogger()
        {
            if (Helper == null)
            {
                Logger.LogError("Logger initialization failed. Helper is null.");
            }

            IAPILog logger = Helper.GetLoggerFactory().GetLogger().ForContext<ScheduleQueueAgentBase>();
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
            Logger.LogError(
                exception,
                "An error occured during finalization of Job with ID: {JobID} in {TypeName}",
                job.JobId,
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

                Logger.LogInformation("Jobs in queue:\n{@jobsInQueue}", jobs.Select(x => x.RemoveSensitiveData()));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to log jobs in queue.");
            }
        }

        #endregion
    }
}
