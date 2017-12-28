using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Castle.Windsor;
using kCura.Agent.CustomAttributes;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.Installer;
using kCura.IntegrationPoints.Agent.Logging;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.TimeMachine;
using Relativity.API;
using ITaskFactory = kCura.IntegrationPoints.Agent.TaskFactory.ITaskFactory;

namespace kCura.IntegrationPoints.Agent
{
	[Name(_AGENT_NAME)]
	[Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)]
	[Description("An agent that manages Integration Point jobs.")]
	public class Agent : ScheduleQueueAgentBase, IDisposable
	{
		private CreateErrorRdo _errorService;
		private IAgentHelper _helper;
		private IAPILog _logger;
		private Lazy<IWindsorContainer> _agentLevelContainer;
		private JobContextProvider _jobContextProvider;
		private IDisposable _jobContext;
		private const string _AGENT_NAME = "Integration Points Agent";

		public Agent() : base(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID))
		{
			JobExecutionError += OnJobExecutionError;
			Apps.Common.Config.Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);

			_agentLevelContainer = new Lazy<IWindsorContainer>(CreateAgentLevelContainer);

#if TIME_MACHINE
			AgentTimeMachineProvider.Current =
				new DefaultAgentTimeMachineProvider(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
#endif
		}

		/// <summary>
		/// Set should be used only for unit/integration tests purpose
		/// </summary>
		public new IAgentHelper Helper
		{
			get { return _helper ?? (_helper = base.Helper); }
			set { _helper = value; }
		}

		public override string Name => _AGENT_NAME;

		protected override void Initialize()
		{
			base.Initialize();
			_logger = Helper.GetLoggerFactory().GetLogger().ForContext<Agent>();
		}

		public override ITask GetTask(Job job)
		{
			IWindsorContainer container = _agentLevelContainer.Value;
			_jobContext = _jobContextProvider.StartJobContext(job);

			ITaskFactory taskFactory = container.Resolve<ITaskFactory>();
			ITask task = taskFactory.CreateTask(job, this);
			container.Release(taskFactory);
			return task;
		}

		protected override void ReleaseTask(ITask task)
		{
			_jobContext?.Dispose();
			_agentLevelContainer.Value?.Release(task);
		}

		protected override void LogJobState(Job job, JobLogState state, Exception exception = null, string details = null)
		{
			if (exception != null)
			{
				details = details ?? string.Empty;
				details += Environment.NewLine;
				details += exception.Message + Environment.NewLine + exception.StackTrace;
			}

			_logger.LogInformation("Integration Points job status update: {@JobLogInformation}",
				new JobLogInformation() { Job = job, State = state, Details = details });
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
		}

		private IWindsorContainer CreateAgentLevelContainer()
		{
			var container = new WindsorContainer();
			container.Install(new AgentAggregatedInstaller(Helper, ScheduleRuleFactory));

			_jobContextProvider = container.Resolve<JobContextProvider>();
			return container;
		}

		private CreateErrorRdo ErrorService => _errorService ?? (_errorService = new CreateErrorRdo(new RsapiClientFactory(Helper), Helper, new SystemEventLoggingService()));

		public void Dispose()
		{
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
	}
}