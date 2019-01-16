using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Castle.Windsor;
using kCura.Agent.CustomAttributes;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.Installer;
using kCura.IntegrationPoints.Agent.Interfaces;
using kCura.IntegrationPoints.Agent.Logging;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.TimeMachine;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Sync;
using IContainer = Autofac.IContainer;
using ITaskFactory = kCura.IntegrationPoints.Agent.TaskFactory.ITaskFactory;

namespace kCura.IntegrationPoints.Agent
{
	[Name(_AGENT_NAME)]
	[Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)]
	[Description("An agent that manages Integration Point jobs.")]
	public class Agent : ScheduleQueueAgentBase, ITaskProvider, IAgentNotifier, IDisposable
	{
		private CreateErrorRdo _errorService;
		private IAgentHelper _helper;
		private IAPILog _logger;
		private JobContextProvider _jobContextProvider;
		private IJobExecutor _jobExecutor;
		private const string _AGENT_NAME = "Integration Points Agent";
		private readonly Lazy<IWindsorContainer> _agentLevelContainer;

		public virtual event ExceptionEventHandler JobExecutionError;

		public Agent() : base(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID))
		{
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
			_jobExecutor = new JobExecutor(this, this, _logger);
			_jobExecutor.JobExecutionError += OnJobExecutionError;
		}

		protected override TaskResult ProcessJob(Job job)
		{
			using (JobContextProvider.StartJobContext(job))
			{
				if (ShouldUseRelativitySync(job))
				{
					IContainer container = CreateAutofacContainer();
					ISyncJob syncJob = CreateSyncJob(job, container);
					syncJob.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
					return new TaskResult { Status = TaskStatusEnum.Success };
				}

				return _jobExecutor.ProcessJob(job);
			}
		}

		private bool ShouldUseRelativitySync(Job job)
		{
			IIntegrationPointService integrationPointService = _agentLevelContainer.Value.Resolve<IIntegrationPointService>();
			IntegrationPoint integrationPoint = integrationPointService.GetRdo(job.RelatedObjectArtifactID);
			IProviderTypeService providerTypeService = _agentLevelContainer.Value.Resolve<IProviderTypeService>();
			ProviderType providerType = providerTypeService.GetProviderType(integrationPoint.SourceProvider ?? 0,
				integrationPoint.DestinationProvider ?? 0);
			if (providerType == ProviderType.Relativity)
			{
				SourceConfiguration sourceConfiguration =
					JsonConvert.DeserializeObject<SourceConfiguration>(integrationPoint.SourceConfiguration);
				ImportSettings destinationConfiguration =
					JsonConvert.DeserializeObject<ImportSettings>(integrationPoint.SourceConfiguration);
				if (sourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch &&
				    !destinationConfiguration.ImageImport &&
				    !destinationConfiguration.ProductionImport)
				{
					return true;
				}
			}

			return false;
		}

		private ISyncJob CreateSyncJob(Job job, IContainer container)
		{
			SyncJobFactory jobFactory = new SyncJobFactory();
			SyncJobParameters parameters = new SyncJobParameters((int) job.JobId, job.WorkspaceID);
			ISyncJob syncJob = jobFactory.Create(container, parameters);
			return syncJob;
		}

		private static IContainer CreateAutofacContainer()
		{
			var containerBuilder = new ContainerBuilder();
			IContainer container = containerBuilder.Build();
			return container;
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

			_logger.LogInformation("Integration Points job status update: {@JobLogInformation}",
				new JobLogInformation { Job = job, State = state, Details = details });
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