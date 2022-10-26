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
using kCura.IntegrationPoints.Common.RelativitySync;
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
using kCura.IntegrationPoints.Domain.Managers;
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
        private const string _AGENT_NAME = "Integration Points Agent";
        private const string _RELATIVITY_SYNC_JOB_TYPE = "Relativity.Sync";

        protected IWindsorContainer Container;

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

        public override string Name => _AGENT_NAME;

        public ITask GetTask(Job job)
        {
            if (Container == null)
            {
                throw new InvalidOperationException("Cannot get task to process because container is not initialized. This is error in Agent.");
            }

            // Because of incredibly bad design of RIP we have to resolve ITaskFactory from container here
            ITask task = Container.Resolve<ITaskFactory>().CreateTask(job, this);
            return task;
        }

        public void NotifyAgent(LogCategory category, string message)
        {
            NotifyAgentTab(category, message);
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        }

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
                Container = CreateAgentLevelContainer();

                using (Container.Resolve<IJobContextProvider>().StartJobContext(job))
                {
                    if (job.JobFailed != null)
                    {
                        MarkJobHistoryAsFailedAsync(Container, job).GetAwaiter().GetResult();
                        return new TaskResult
                        {
                            Status = TaskStatusEnum.Fail,
                            Exceptions = new List<Exception> { job.JobFailed.Exception }
                        };
                    }

                    using (StartMemoryUsageMetricReporting(Container, job))
                    using (StartHeartbeatReporting(Container, job))
                    {
                        if (ShouldUseRelativitySync(Container, job))
                        {
                            try
                            {
                                Container.Register(Component.For<Job>().UsingFactoryMethod(k => job)
                                    .Named($"{job.JobId}-{Guid.NewGuid()}")); // ???

                                RelativitySyncAdapter syncAdapter = Container.Resolve<RelativitySyncAdapter>();
                                IAPILog logger = Container.Resolve<IAPILog>();
                                AgentCorrelationContext correlationContext = GetCorrelationContext(Container, job);
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

                                MarkJobAsFailed(Container, job, ex);

                                return new TaskResult
                                {
                                    Status = TaskStatusEnum.Fail,
                                    Exceptions = new[] { ex }
                                };
                            }
                            finally
                            {
                                IJobStopManager jobStopManager = Container.Resolve<IJobStopManager>();
                                jobStopManager.Dispose();
                            }
                        }
                        else
                        {
                            SendJobStartedMessage(Container, job);
                            TaskResult result = JobExecutor.ProcessJob(job);
                            return result;
                        }
                    }
                }
            }
            finally
            {
                Container.Dispose();
                Container = null;
            }
        }

        private IDisposable StartMemoryUsageMetricReporting(IWindsorContainer container, Job job)
        {
            return container.Resolve<IMemoryUsageReporter>()
                .ActivateTimer(job.JobId, GetCorrelationId(job, container.Resolve<ISerializer>()), job.TaskType);
        }

        private IDisposable StartHeartbeatReporting(IWindsorContainer container, Job job)
        {
            return container.Resolve<IHeartbeatReporter>()
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

        private async Task MarkJobHistoryAsFailedAsync(IWindsorContainer container, Job job)
        {
            IntegrationPoint integrationPoint = await container.Resolve<IIntegrationPointRepository>()
                .ReadAsync(job.RelatedObjectArtifactID).ConfigureAwait(false);
            if (integrationPoint == null)
            {
                throw new NullReferenceException(
                    $"Unable to retrieve the integration point for the following job: {job.JobId}");
            }

            ITaskFactoryJobHistoryService jobHistoryService =
                container.Resolve<ITaskFactoryJobHistoryServiceFactory>()
                    .CreateJobHistoryService(integrationPoint);
            jobHistoryService.SetJobIdOnJobHistory(job);
            jobHistoryService.UpdateJobHistoryOnFailure(job, job.JobFailed.Exception);
        }

        private AgentCorrelationContext GetCorrelationContext(IWindsorContainer container, Job job)
        {
            ITaskParameterHelper taskParameterHelper = container.Resolve<ITaskParameterHelper>();
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

        private void SendJobStartedMessage(IWindsorContainer container, Job job)
        {
            try
            {
                ITaskParameterHelper taskParameterHelper = container.Resolve<ITaskParameterHelper>();
                IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();
                IProviderTypeService providerTypeService = container.Resolve<IProviderTypeService>();
                IMessageService messageService = container.Resolve<IMessageService>();

                Guid batchInstanceId = taskParameterHelper.GetBatchInstance(job);
                Logger.LogInformation("Job will be executed in case of BatchInstanceId: {batchInstanceId}", batchInstanceId);
                if (!IsJobResumed(container, batchInstanceId))
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

        private bool IsJobResumed(IWindsorContainer container, Guid batchInstanceId)
        {
            IJobHistoryService jobHistoryService = container.Resolve<IJobHistoryService>();
            JobHistory jobHistory = jobHistoryService.GetRdoWithoutDocuments(batchInstanceId);
            ChoiceRef jobHistoryStatus = jobHistory?.JobStatus;
            if (jobHistoryStatus == null)
            {
                return false;
            }

            return jobHistoryStatus.EqualsToChoice(JobStatusChoices.JobHistorySuspended);
        }

        private bool ShouldUseRelativitySync(IWindsorContainer container, Job job)
        {
            IRelativitySyncConstrainsChecker constrainsChecker = container.Resolve<IRelativitySyncConstrainsChecker>();
            return constrainsChecker.ShouldUseRelativitySync(job.RelatedObjectArtifactID);
        }

        private void MarkJobAsFailed(IWindsorContainer container, Job job, Exception ex)
        {
            try
            {
                IExtendedJob syncJob = container.Resolve<IExtendedJob>();
                if (syncJob != null)
                {
                    IJobHistorySyncService jobHistorySyncService = container.Resolve<IJobHistorySyncService>();
                    jobHistorySyncService.MarkJobAsFailedAsync(syncJob, ex).GetAwaiter().GetResult();
                }
            }
            catch (Exception ie)
            {
                Logger.LogError(ie, "Unable to mark Sync job as failed and log a job history error in {WorkspaceArtifactId} for {JobId}.", job.WorkspaceID, job.JobId);
            }
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

        protected virtual IWindsorContainer CreateAgentLevelContainer()
        {
            var container = new WindsorContainer();
            container.Install(new AgentAggregatedInstaller(Helper, ScheduleRuleFactory));
            container.Install(new RelativitySyncInstaller());
            container.Register(Component.For<IRemovableAgent>().Instance(this));
            return container;
        }

        private ErrorService ErrorService => _errorService ?? (_errorService = new ErrorService(Helper, new SystemEventLoggingService()));

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
