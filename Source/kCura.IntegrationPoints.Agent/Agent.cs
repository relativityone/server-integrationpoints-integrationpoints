using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Castle.Windsor;
using kCura.Agent.CustomAttributes;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.Installer;
using kCura.IntegrationPoints.Agent.Interfaces;
using kCura.IntegrationPoints.Agent.Logging;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.RelativitySync;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.TimeMachine;
using kCura.ScheduleQueue.Core.Validation;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Component = Castle.MicroKernel.Registration.Component;

namespace kCura.IntegrationPoints.Agent
{
	[Name(_AGENT_NAME)]
	[Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)]
	[Description("An agent that manages Integration Point jobs.")]
	public class Agent : ScheduleQueueAgentBase, ITaskProvider, IAgentNotifier, IRemovableAgent, IDisposable
	{
		private ErrorService _errorService;
		private IAgentHelper _helper;
		private IJobContextProvider _jobContextProvider;
		private IJobExecutor _jobExecutor;
		private const string _AGENT_NAME = "Integration Points Agent";
		private const string _RELATIVITY_SYNC_JOB_TYPE = "Relativity.Sync";
		private readonly Lazy<IWindsorContainer> _agentLevelContainer;

		public virtual event ExceptionEventHandler JobExecutionError;

		public Agent() : base(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID))
		{
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);

			_agentLevelContainer = new Lazy<IWindsorContainer>(CreateAgentLevelContainer);

#if TIME_MACHINE
			AgentTimeMachineProvider.Current =
				new DefaultAgentTimeMachineProvider(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
#endif
		}

		protected Agent(Guid agentGuid, IAgentService agentService = null, IJobService jobService = null,
				IScheduleRuleFactory scheduleRuleFactory = null, IQueueJobValidator queueJobValidator = null,
				IQueueQueryManager queryManager = null, IAPILog logger = null) 
			: base(agentGuid, agentService, 
				jobService, scheduleRuleFactory, queueJobValidator, queryManager, logger)
		{
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);

			_agentLevelContainer = new Lazy<IWindsorContainer>(CreateAgentLevelContainer);

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
			_jobExecutor = new JobExecutor(this, this, Logger);
			_jobExecutor.JobExecutionError += OnJobExecutionError;
			_jobExecutor.JobPostExecute += OnJobPostExecute;
		}

		protected override TaskResult ProcessJob(Job job)
		{
			SetWebApiTimeout();

			using (IWindsorContainer ripContainerForSync = CreateAgentLevelContainer())
			using (ripContainerForSync.Resolve<IJobContextProvider>().StartJobContext(job))
			{
				if (ShouldUseRelativitySync(job, ripContainerForSync))
				{
					ripContainerForSync.Register(Component.For<Job>().UsingFactoryMethod(k => job).Named($"{job.JobId}-{Guid.NewGuid()}"));
					try
					{
						RelativitySyncAdapter syncAdapter = ripContainerForSync.Resolve<RelativitySyncAdapter>();
						IAPILog logger = ripContainerForSync.Resolve<IAPILog>();
						AgentCorrelationContext correlationContext = GetCorrelationContext(job);
						using (logger.LogContextPushProperties(correlationContext))
						{
							return syncAdapter.RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();
						}
					}
					catch (Exception e)
					{
						//Not much we can do here. If container failed we're unable to do anything.
						//Exception was thrown from container, because RelativitySyncAdapter catches all exceptions inside
						Logger.LogError(e, $"Unable to resolve {nameof(RelativitySyncAdapter)}.");

						try
						{
							IExtendedJob syncJob = ripContainerForSync.Resolve<IExtendedJob>();
							if (syncJob != null)
							{
								IJobHistorySyncService jobHistorySyncService = ripContainerForSync.Resolve<IJobHistorySyncService>();
								jobHistorySyncService.MarkJobAsFailedAsync(syncJob, e).ConfigureAwait(false).GetAwaiter().GetResult();
							}
						}
						catch (Exception ie)
						{
							Logger.LogError(ie, "Unable to mark Sync job as failed and log a job history error in {WorkspaceArtifactId} for {JobId}.", job.WorkspaceID, job.JobId);
						}

						return new TaskResult
						{
							Status = TaskStatusEnum.Fail,
							Exceptions = new[] { e }
						};
					}
				}
			}

			using (JobContextProvider.StartJobContext(job))
			{
				SendJobStartedMessage(job);
				TaskResult result = _jobExecutor.ProcessJob(job);
				return result;
			}
		}

		private void SetWebApiTimeout()
		{
			TimeSpan? timeout = Config.Config.Instance.RelativityWebApiTimeout;
			if (timeout.HasValue)
			{
				int timeoutMs = (int)timeout.Value.TotalMilliseconds;
				kCura.WinEDDS.Service.Settings.DefaultTimeOut = timeoutMs;
				Logger.LogInformation("Relativity WebAPI timeout set to {timeout}ms.", timeoutMs);
			}
		}

		private static AgentCorrelationContext GetCorrelationContext(Job job)
		{
			var correlationContext = new AgentCorrelationContext
			{
				JobId = job.JobId,
				RootJobId = job.RootJobId,
				WorkspaceId = job.WorkspaceID,
				UserId = job.SubmittedBy,
				IntegrationPointId = job.RelatedObjectArtifactID,
				ActionName = _RELATIVITY_SYNC_JOB_TYPE
			};
			return correlationContext;
		}

		private void SendJobStartedMessage(Job job)
		{
			TaskParameterHelper taskParameterHelper = _agentLevelContainer.Value.Resolve<TaskParameterHelper>();
			IIntegrationPointService integrationPointService = _agentLevelContainer.Value.Resolve<IIntegrationPointService>();
			IProviderTypeService providerTypeService = _agentLevelContainer.Value.Resolve<IProviderTypeService>();
			IMessageService messageService = _agentLevelContainer.Value.Resolve<IMessageService>();

			Guid batchInstanceId = taskParameterHelper.GetBatchInstance(job);
			IntegrationPoint integrationPoint = integrationPointService.ReadIntegrationPoint(job.RelatedObjectArtifactID);
			var message = new JobStartedMessage
			{
				Provider = integrationPoint.GetProviderName(providerTypeService),
				CorrelationID = batchInstanceId.ToString()
			};
			messageService.Send(message).GetAwaiter().GetResult();
		}

		private bool ShouldUseRelativitySync(Job job, IWindsorContainer ripContainerForSync)
		{
			try
			{
				RelativitySyncConstrainsChecker constrainsChecker = ripContainerForSync.Resolve<RelativitySyncConstrainsChecker>();
				return constrainsChecker.ShouldUseRelativitySync(job);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex,
					"Error occurred when trying to determine if Relativity Sync should be used. RIP will use old logic instead.");
			}

			return false;
		}

		public ITask GetTask(Job job)
		{
			IWindsorContainer container = _agentLevelContainer.Value;
			ITaskFactory taskFactory = container.Resolve<ITaskFactory>();
			ITask task = taskFactory.CreateTask(job, this);
			container.Release(taskFactory);
			return task;
		}

		public void ReleaseTask(ITask task)
		{
			if (task != null)
			{
				_agentLevelContainer.Value?.Release(task);
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

		protected TaskResult OnJobPostExecute(Job job)
		{
			IWindsorContainer container = _agentLevelContainer.Value;
			ISerializer serializer = container.Resolve<ISerializer>();
			IJobHistoryService jobHistoryService = container.Resolve<IJobHistoryService>();

			TaskParameters taskParameters = serializer.Deserialize<TaskParameters>(job.JobDetails);
			JobHistory jobHistory = jobHistoryService.GetRdoWithoutDocuments(taskParameters.BatchInstance);
			if (jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistorySuspended))
			{
				return new TaskResult { Status = TaskStatusEnum.DrainStopped, Exceptions = null };
			}

			return new TaskResult { Status = TaskStatusEnum.Success, Exceptions = null };
		}

		protected IJobContextProvider JobContextProvider
		{
			get
			{
				if (_jobContextProvider == null)
				{
					_jobContextProvider = _agentLevelContainer.Value.Resolve<IJobContextProvider>();
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
			if (_agentLevelContainer.IsValueCreated)
			{
				if (_jobContextProvider != null)
				{
					_agentLevelContainer.Value?.Release(_jobContextProvider);
				}

				_agentLevelContainer.Value?.Dispose();
			}
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