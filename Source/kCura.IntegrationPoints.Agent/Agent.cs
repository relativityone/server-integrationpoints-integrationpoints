using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.Agent.CustomAttributes;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
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
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.RelativitySync;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.TimeMachine;
using kCura.ScheduleQueue.Core.Validation;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.Services.Choice;
using Relativity.Telemetry.APM;
using Choice = Relativity.Services.Objects.DataContracts.ChoiceRef;
using Component = Castle.MicroKernel.Registration.Component;

namespace kCura.IntegrationPoints.Agent
{
    [Name(_AGENT_NAME)]
    [Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)]
    [System.ComponentModel.Description("An agent that manages Integration Point jobs.")]
    [WorkloadDiscovery.CustomAttributes.Path("Relativity.Rest/api/Relativity.IntegrationPoints.Services.IIntegrationPointsModule/Integration%20Points%20Agent/GetWorkloadAsync")]
    public class Agent : ScheduleQueueAgentBase, ITaskProvider, IRemovableAgent, IDisposable
    {
        private ErrorService _errorService;
        private const string _AGENT_NAME = "Integration Points Agent";
        private const string _RELATIVITY_SYNC_JOB_TYPE = "Relativity.Sync";
        private const int _MAX_COMPLETION_PORT_THREADS = 100;

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
            IAPM apm = null,
            IDbContextFactory dbContextFactory = null,
            IRelativityObjectManagerFactory relativityObjectManagerFactory = null)
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
                apm,
                dbContextFactory,
                relativityObjectManagerFactory)
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

        public void Dispose()
        {
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        }

        protected override void Initialize()
        {
            base.Initialize();
            JobExecutor = new JobExecutor(this, JobService, Logger);
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
                        if (job.JobFailed.ShouldBreakSchedule)
                        {
                            IIntegrationPointService integrationPointService = Container.Resolve<IIntegrationPointService>();
                            integrationPointService.DisableScheduler(job.RelatedObjectArtifactID);
                        }

                        MarkJobAsFailedAsync(Container, job, job.JobFailed.Exception)
                            .GetAwaiter().GetResult();
                        return new TaskResult
                        {
                            Status = TaskStatusEnum.Fail,
                            Exceptions = new List<Exception> { job.JobFailed.Exception }
                        };
                    }

                    using (StartMemoryUsageMetricReporting(Container, job))
                    using (StartHeartbeatReporting(Container, job))
                    {
                        SendJobStartedMessage(Container, job);
                        TaskResult result = JobExecutor.ProcessJob(job);
                        return result;
                    }
                }
            }
            finally
            {
                Container?.Dispose();
                Container = null;
            }
        }

        private IDisposable StartMemoryUsageMetricReporting(IWindsorContainer container, Job job)
        {
            return container.Resolve<IMemoryUsageReporter>()
                .ActivateTimer(job.JobId, job.CorrelationID, job.TaskType);
        }

        private IDisposable StartHeartbeatReporting(IWindsorContainer container, Job job)
        {
            return container.Resolve<IHeartbeatReporter>()
                .ActivateHeartbeat(job.JobId);
        }

        private void SendJobStartedMessage(IWindsorContainer container, Job job)
        {
            try
            {
                IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();
                IProviderTypeService providerTypeService = container.Resolve<IProviderTypeService>();
                IMessageService messageService = container.Resolve<IMessageService>();

                Logger.LogInformation("Job will be executed in case of BatchInstanceId: {batchInstanceId}", job.CorrelationID);
                //TODO change Guid to string 
                if (!IsJobResumed(container, Guid.Parse(job.CorrelationID)))
                {
                    IntegrationPointSlimDto integrationPoint = integrationPointService.ReadSlim(job.RelatedObjectArtifactID);
                    var message = new JobStartedMessage
                    {
                        Provider = integrationPoint.GetProviderName(providerTypeService),
                        CorrelationID = job.CorrelationID
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

        private async Task MarkJobAsFailedAsync(IWindsorContainer container, Job job, Exception ex)
        {
            try
            {
                IExtendedJob extendedJob = container.Resolve<IExtendedJob>();
                if (extendedJob != null)
                {
                    IJobHistorySyncService jobHistoryService = container.Resolve<IJobHistorySyncService>();

                    Choice failedChoice = new Choice
                    {
                        Guid = JobStatusChoices.JobHistoryErrorJobFailed.Guids[0]
                    };

                    await jobHistoryService.UpdateFinishedJobAsync(extendedJob, failedChoice, true).ConfigureAwait(false);
                    await jobHistoryService.AddJobHistoryErrorAsync(extendedJob, ex).ConfigureAwait(false);
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

        private ErrorService ErrorService => _errorService ?? (_errorService = new ErrorService(Helper));

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
