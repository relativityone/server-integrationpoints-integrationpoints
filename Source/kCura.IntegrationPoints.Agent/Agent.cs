using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.Agent.CustomAttributes;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.Installer;
using kCura.IntegrationPoints.Agent.Interfaces;
using kCura.IntegrationPoints.Agent.Logging;
using kCura.IntegrationPoints.Agent.Monitoring.HearbeatReporter;
using kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.RelativitySync;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.TimeMachine;
using kCura.ScheduleQueue.Core.Validation;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.Services.Choice;
using Relativity.Services.Exceptions;
using Relativity.Telemetry.APM;
using Component = Castle.MicroKernel.Registration.Component;

namespace kCura.IntegrationPoints.Agent
{
    [Name(_AGENT_NAME)]
    [Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)]
    [System.ComponentModel.Description("An agent that manages Integration Point jobs.")]
    [WorkloadDiscovery.CustomAttributes.Path("Relativity.Rest/api/Relativity.IntegrationPoints.Services.IIntegrationPointsModule/Integration%20Points%20Agent/GetWorkloadAsync")]
    public class Agent : ScheduleQueueAgentBase, ITaskProvider, IAgentNotifier, IRemovableAgent, IDisposable
    {
        private ErrorService _errorService;
        private IAgentHelper _helper;
        private IJobContextProvider _jobContextProvider;
        private const string _AGENT_NAME = "Integration Points Agent";
        private const string _RELATIVITY_SYNC_JOB_TYPE = "Relativity.Sync";

        private IWindsorContainer _container;
        private T Resolve<T>() => _container.Resolve<T>();

        internal IJobExecutor JobExecutor { get; set; }

        public virtual event ExceptionEventHandler JobExecutionError;

        public Agent() : base(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID))
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);

#if TIME_MACHINE
            AgentTimeMachineProvider.Current =
                new DefaultAgentTimeMachineProvider(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
#endif
        }

        protected Agent(
            Guid agentGuid,
            IAgentService agentService = null,
            IJobService jobService = null,
            IScheduleRuleFactory scheduleRuleFactory = null,
            IQueueJobValidator queueJobValidator = null,
            IQueueQueryManager queryManager = null,
            IKubernetesMode kubernetesMode = null,
            IDateTime dateTime = null,
            IAPILog logger = null,
            IConfig config = null,
            IAPM apm = null)
            : base(
                agentGuid,
                kubernetesMode,
                agentService,
                jobService,
                scheduleRuleFactory,
                queueJobValidator,
                queryManager,
                dateTime,
                logger,
                config,
                apm)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);

#if TIME_MACHINE
            AgentTimeMachineProvider.Current =
                new DefaultAgentTimeMachineProvider(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
#endif
        }

        /// <summary>
        ///     Set should be used only for unit/integration tests purpose
        /// </summary>
        public new IAgentHelper Helper
        {
            get => _helper ?? (_helper = base.Helper);
            set => _helper = value;
        }

        public override string Name => _AGENT_NAME;

        protected override void Initialize()
        {
            base.Initialize();
            JobExecutor = new JobExecutor(this, this, JobService, Logger);
            JobExecutor.JobExecutionError += OnJobExecutionError;
        }

        protected override TaskResult ProcessJob(Job job)
        {
            try
            {
                _container = CreateAgentLevelContainer();

                if (job.JobFailed != null)
                {
                    MarkJobHistoryAsFailedAsync(job).GetAwaiter().GetResult();
                    return new TaskResult
                    {
                        Status = TaskStatusEnum.Fail,
                        Exceptions = new List<Exception> { job.JobFailed.Exception }
                    };
                }

                using (StartMemoryUsageMetricReporting(job))
                using (StartHeartbeatReporting(job))
                {
                    using (Resolve<IJobContextProvider>().StartJobContext(job))
                    {
                        if (ShouldUseRelativitySync(job))
                        {
                            _container.Register(Component.For<Job>().UsingFactoryMethod(k => job).Named($"{job.JobId}-{Guid.NewGuid()}"));

                            try
                            {
                                RelativitySyncAdapter syncAdapter = Resolve<RelativitySyncAdapter>();
                                IAPILog logger = Resolve<IAPILog>();
                                AgentCorrelationContext correlationContext = GetCorrelationContext(job);
                                using (logger.LogContextPushProperties(correlationContext))
                                {
                                    return syncAdapter.RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                                }
                            }
                            catch (Exception ex)
                            {
                                // Not much we can do here. If container failed we're unable to do anything.
                                // Exception was thrown from container, because RelativitySyncAdapter catches all exceptions inside
                                Logger.LogError(ex, $"Unable to resolve {nameof(RelativitySyncAdapter)}.");

                                MarkJobAsFailed(job, _container, ex);

                                return new TaskResult
                                {
                                    Status = TaskStatusEnum.Fail,
                                    Exceptions = new[] { ex }
                                };
                            }
                        }
                    }

                    using (JobContextProvider.StartJobContext(job))
                    {
                        SendJobStartedMessage(job);
                        TaskResult result = JobExecutor.ProcessJob(job);
                        return result;
                    }
                }
            }
            finally
            {
                _container?.Dispose();
            }
        }

        private IDisposable StartMemoryUsageMetricReporting(Job job)
        {
            return Resolve<IMemoryUsageReporter>()
                .ActivateTimer(job.JobId, GetCorrelationId(job, Resolve<ISerializer>()), job.TaskType);
        }

        private IDisposable StartHeartbeatReporting(Job job)
        {
            return Resolve<IHeartbeatReporter>()
                .ActivateHeartbeat(job.JobId);
        }

        private string GetCorrelationId(Job job, ISerializer serializer)
        {
            string result = string.Empty;
            try
            {
                TaskParameters taskParameters = serializer.Deserialize<TaskParameters>(job.JobDetails);
                result = taskParameters.BatchInstance.ToString();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error occurred while retrieving batch instance for job: {jobId}", job.JobId);
            }
            return result;
        }

        private async Task MarkJobHistoryAsFailedAsync(Job job)
        {
            using (JobContextProvider.StartJobContext(job))
            {
                IntegrationPoint integrationPoint = await Resolve<IIntegrationPointRepository>()
                    .ReadAsync(job.RelatedObjectArtifactID).ConfigureAwait(false);
                if (integrationPoint == null)
                {
                    throw new NullReferenceException(
                        $"Unable to retrieve the integration point for the following job: {job.JobId}");
                }

                ITaskFactoryJobHistoryService jobHistoryService =
                    Resolve<ITaskFactoryJobHistoryServiceFactory>()
                        .CreateJobHistoryService(integrationPoint);
                jobHistoryService.SetJobIdOnJobHistory(job);
                jobHistoryService.UpdateJobHistoryOnFailure(job, job.JobFailed.Exception);
            }
        }

        private AgentCorrelationContext GetCorrelationContext(Job job)
        {
            ITaskParameterHelper taskParameterHelper = Resolve<ITaskParameterHelper>();
            Guid batchInstanceId = taskParameterHelper.GetBatchInstance(job);
            string correlationId = batchInstanceId.ToString();

            var correlationContext = new AgentCorrelationContext
            {
                JobId = job.JobId,
                RootJobId = job.RootJobId,
                WorkspaceId = job.WorkspaceID,
                UserId = job.SubmittedBy,
                IntegrationPointId = job.RelatedObjectArtifactID,
                ActionName = _RELATIVITY_SYNC_JOB_TYPE,
                WorkflowId = correlationId
            };
            return correlationContext;
        }

        private void SendJobStartedMessage(Job job)
        {
            try
            {
                ITaskParameterHelper taskParameterHelper = Resolve<ITaskParameterHelper>();
                IIntegrationPointService integrationPointService = Resolve<IIntegrationPointService>();
                IProviderTypeService providerTypeService = Resolve<IProviderTypeService>();
                IMessageService messageService = Resolve<IMessageService>();

                Guid batchInstanceId = taskParameterHelper.GetBatchInstance(job);
                Logger.LogInformation("Job will be executed in case of BatchInstanceId: {batchInstanceId}", batchInstanceId);
                if (!IsJobResumed(batchInstanceId))
                {
                    IntegrationPoint integrationPoint = integrationPointService.ReadIntegrationPoint(job.RelatedObjectArtifactID);
                    var message = new JobStartedMessage
                    {
                        Provider = integrationPoint.GetProviderName(providerTypeService),
                        CorrelationID = batchInstanceId.ToString()
                    };
                    messageService.Send(message).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception occurred during sending Job Start metric for JobId {jobId}", job.JobId);
            }
        }

        private bool IsJobResumed(Guid batchInstanceId)
        {
            IJobHistoryService jobHistoryService = Resolve<IJobHistoryService>();
            JobHistory jobHistory = jobHistoryService.GetRdoWithoutDocuments(batchInstanceId);
            ChoiceRef jobHistoryStatus = jobHistory?.JobStatus;
            if (jobHistoryStatus == null)
            {
                return false;
            }

            return jobHistoryStatus.EqualsToChoice(JobStatusChoices.JobHistorySuspended);
        }

        private bool ShouldUseRelativitySync(Job job)
        {
            IRelativitySyncConstrainsChecker constrainsChecker = _container.Resolve<IRelativitySyncConstrainsChecker>();
            return constrainsChecker.ShouldUseRelativitySync(job);
        }

        private void MarkJobAsFailed(Job job, IWindsorContainer ripContainerForSync, Exception ex)
        {
            try
            {
                IExtendedJob syncJob = ripContainerForSync.Resolve<IExtendedJob>();
                if (syncJob != null)
                {
                    IJobHistorySyncService jobHistorySyncService = ripContainerForSync.Resolve<IJobHistorySyncService>();
                    jobHistorySyncService.MarkJobAsFailedAsync(syncJob, ex).GetAwaiter().GetResult();
                }
            }
            catch (Exception ie)
            {
                Logger.LogError(ie, "Unable to mark Sync job as failed and log a job history error in {WorkspaceArtifactId} for {JobId}.", job.WorkspaceID, job.JobId);
            }
        }

        public ITask GetTask(Job job)
        {
            ITaskFactory taskFactory = _container.Resolve<ITaskFactory>();
            ITask task = taskFactory.CreateTask(job, this);
            _container.Release(taskFactory);
            return task;
        }

        public void ReleaseTask(ITask task)
        {
            if (task != null)
            {
                _container.Release(task);
            }
        }

        public void NotifyAgent(LogCategory category, string message)
        {
            NotifyAgentTab(category, message);
        }

        protected override void LogJobState(Job job, JobLogState state, Exception exception = null, string details = null)
        {
            if (exception != null)
            {
                details = details ?? string.Empty;
                details += Environment.NewLine;
                details += exception.Message + Environment.NewLine + exception.StackTrace;
            }

            Logger.LogInformation("Integration Points job status update: {@JobLogInformation}", new JobLogInformation
            {
                Job = job.RemoveSensitiveData(),
                State = state,
                Details = details
            });
        }

        protected void OnJobExecutionError(Job job, ITask task, Exception exception)
        {
            LogJobExecutionError(job, exception);
            LogJobState(job, JobLogState.Error, exception);
            var integrationPointsException = exception as IntegrationPointsException;
            if (integrationPointsException != null)
            {
                ErrorService.LogError(job, integrationPointsException);
            }
            else
            {
                ErrorService.LogError(job, exception, _AGENT_NAME);
            }

            JobExecutionError?.Invoke(job, task, exception);
        }

        protected IJobContextProvider JobContextProvider
        {
            get
            {
                if (_jobContextProvider == null)
                {
                    _jobContextProvider = Resolve<IJobContextProvider>();
                }

                return _jobContextProvider;
            }
        }

        protected virtual IWindsorContainer CreateAgentLevelContainer()
        {
            var container = new WindsorContainer();
            container.Install(new AgentAggregatedInstaller(Helper, ScheduleRuleFactory));
            container.Install(new RelativitySyncInstaller());
            container.Register(Component.For<IRemovableAgent>().Instance(this));
            return container;
        }

        private ErrorService ErrorService => _errorService ?? (_errorService = new ErrorService(Helper, new SystemEventLoggingService()));

        public void Dispose()
        {
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

            if (_jobContextProvider != null)
            {
                _container.Release(_jobContextProvider);
            }

            _container.Dispose();
        }


        private void LogJobExecutionError(Job job, Exception exception)
        {
            Logger.LogError(exception, "An error occured during execution of Job with ID: {JobID} in {TypeName}", job.JobId,
                nameof(ScheduleQueueAgentBase));
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.LogFatal(e.ExceptionObject as Exception, "Unhandled exception occurred!");
        }
    }
}