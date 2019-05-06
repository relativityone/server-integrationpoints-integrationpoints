using System;
using System.Runtime.InteropServices;
using Castle.Windsor;
using kCura.Agent.CustomAttributes;
using kCura.Apps.Common.Config;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.Installer;
using kCura.IntegrationPoints.Agent.Interfaces;
using kCura.IntegrationPoints.Agent.Logging;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.RelativitySync;
using kCura.IntegrationPoints.RelativitySync.RipOverride;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.TimeMachine;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Component = Castle.MicroKernel.Registration.Component;

namespace kCura.IntegrationPoints.Agent
{
	[Name(_AGENT_NAME)]
	[Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)]
	[System.ComponentModel.Description("An agent that manages Integration Point jobs.")]
	public class Agent : ScheduleQueueAgentBase, ITaskProvider, IAgentNotifier, IDisposable
	{
		private CreateErrorRdo _errorService;
		private IAgentHelper _helper;
		private JobContextProvider _jobContextProvider;
		private IJobExecutor _jobExecutor;
		private const string _AGENT_NAME = "Integration Points Agent";
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
		}

		protected override TaskResult ProcessJob(Job job)
		{
			using (IWindsorContainer ripContainerForSync = CreateAgentLevelContainer())
			{
				using (ripContainerForSync.Resolve<JobContextProvider>().StartJobContext(job))
				{
					if (ShouldUseRelativitySync(job, ripContainerForSync))
					{
						try
						{
							ripContainerForSync.Register(Component.For<IExtendedJob>().ImplementedBy<ExtendedJob>());
							ripContainerForSync.Register(Component.For<RelativitySyncAdapter>().ImplementedBy<RelativitySyncAdapter>());
							ripContainerForSync.Register(Component.For<IWindsorContainer>().Instance(ripContainerForSync));
							ripContainerForSync.Register(Component.For<ISendEmailWorker>().UsingFactoryMethod(k => k.Resolve<SendEmailWorker>()));
							ripContainerForSync.Register(Component.For<IExportServiceManager>().ImplementedBy<ExportServiceManager>().Named(Guid.NewGuid().ToString()).IsDefault());

							RelativitySyncAdapter syncAdapter = ripContainerForSync.Resolve<RelativitySyncAdapter>();
							return syncAdapter.RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();
						}
						catch (Exception e)
						{
							//Not much we can do here. If container failed we're unable to do anything.
							//Exception was thrown from container, because RelativitySyncAdapter catches all exceptions inside
							Logger.LogError(e, $"Unable to resolve {nameof(RelativitySyncAdapter)}.");
							return new TaskResult
							{
								Status = TaskStatusEnum.Fail,
								Exceptions = new[] {e}
							};
						}
					}
				}
			}

			using (JobContextProvider.StartJobContext(job))
			{
				// If the JobHistory object for this job has already been created by the API (e.g. if a user clicks
				// the "Run" button on an IP), then the JobStarted message will have been raised in the web process
				// instead of the agent process, and we won't have entirely accurate metrics. Therefore we send the
				// same message here as well.
				SendJobStartedMessage(job);

				return _jobExecutor.ProcessJob(job);
			}
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
				Provider = integrationPoint.GetProviderType(providerTypeService).ToString(),
				CorrelationID = batchInstanceId.ToString()
			};
			messageService.Send(message).GetAwaiter().GetResult();
		}

		private bool ShouldUseRelativitySync(Job job, IWindsorContainer ripContainerForSync)
		{
			try
			{
				ripContainerForSync.Register(Component.For<RelativitySyncConstrainsChecker>().ImplementedBy<RelativitySyncConstrainsChecker>());
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

		public void NotifyAgent(int level, LogCategory category, string message)
		{
			NotifyAgentTab(level, category, message);
		}

		protected override void LogJobState(Job job, JobLogState state, Exception exception = null, string details = null)
		{
			if (exception != null)
			{
				details = details ?? string.Empty;
				details += Environment.NewLine;
				details += exception.Message + Environment.NewLine + exception.StackTrace;
			}

			Logger.LogInformation("Integration Points job status update: {@JobLogInformation}",
				new JobLogInformation {Job = job, State = state, Details = details});
		}

		protected void OnJobExecutionError(Job job, ITask task, Exception exception)
		{
			LogJobExecutionError(job, exception);
			LogJobState(job, JobLogState.Error, exception);
			var integrationPointsException = exception as IntegrationPointsException;
			if (integrationPointsException != null)
			{
				ErrorService.Execute(job, integrationPointsException);
			}
			else
			{
				ErrorService.Execute(job, exception, _AGENT_NAME);
			}

			JobExecutionError?.Invoke(job, task, exception);
		}

		protected JobContextProvider JobContextProvider
		{
			get
			{
				if (_jobContextProvider == null)
				{
					_jobContextProvider = _agentLevelContainer.Value.Resolve<JobContextProvider>();
				}

				return _jobContextProvider;
			}
		}

		private IWindsorContainer CreateAgentLevelContainer()
		{
			var container = new WindsorContainer();
			container.Install(new AgentAggregatedInstaller(Helper, ScheduleRuleFactory));
			return container;
		}

		private CreateErrorRdo ErrorService => _errorService ?? (_errorService = new CreateErrorRdo(new RsapiClientWithWorkspaceFactory(Helper), Helper, new SystemEventLoggingService()));

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