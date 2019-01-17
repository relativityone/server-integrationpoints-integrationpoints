﻿using System;
using System.Runtime.InteropServices;
using Castle.Windsor;
using kCura.Agent.CustomAttributes;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Agent.Context;
using kCura.IntegrationPoints.Agent.Installer;
using kCura.IntegrationPoints.Agent.Interfaces;
using kCura.IntegrationPoints.Agent.Logging;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.RelativitySync;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.TimeMachine;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Toggles;
using ITaskFactory = kCura.IntegrationPoints.Agent.TaskFactory.ITaskFactory;

namespace kCura.IntegrationPoints.Agent
{
	[Name(_AGENT_NAME)]
	[Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)]
	[System.ComponentModel.Description("An agent that manages Integration Point jobs.")]
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
					return RelativitySyncAdapter.Run(job);
				}
				return _jobExecutor.ProcessJob(job);
			}
		}

		private bool ShouldUseRelativitySync(Job job)
		{
			_logger.LogInformation("Checking if Relativity Sync flow should be used for job with ID: {jobId}. IntegrationPointId: {integrationPointId}", job.JobId);

			if (!IsRelativitySyncToggleEnabled())
			{
				_logger.LogInformation(
					$"Normal flow will be used for job with ID: {{jobId}} because {nameof(EnableSyncToggle)} is disabled. IntegrationPointId: {{integrationPointId}}",
					job.JobId, job.RelatedObjectArtifactID);

				return false;
			}

			IntegrationPoint integrationPoint = GetIntegrationPoint(job.RelatedObjectArtifactID);
			ProviderType providerType = GetProviderType(integrationPoint.SourceProvider ?? 0, integrationPoint.DestinationProvider ?? 0);
			if (providerType == ProviderType.Relativity)
			{
				SourceConfiguration sourceConfiguration =
					JsonConvert.DeserializeObject<SourceConfiguration>(integrationPoint.SourceConfiguration);
				ImportSettings destinationConfiguration =
					JsonConvert.DeserializeObject<ImportSettings>(integrationPoint.SourceConfiguration);

				if (ConfigurationAllowsUsingRelativitySync(sourceConfiguration, destinationConfiguration))
				{
					_logger.LogInformation("Relativity Sync flow will be used for job with ID: {jobId}. IntegrationPointId: {integrationPointId}", job.JobId, job.RelatedObjectArtifactID);
					return true;
				}
			}

			_logger.LogInformation(
				"Normal flow will be used for job with ID: {jobId} because this integration point does not meet conditions required for running Relativity Sync. IntegrationPointId: {integrationPointId}",
				job.JobId, job.RelatedObjectArtifactID);
			return false;
		}

		private bool ConfigurationAllowsUsingRelativitySync(SourceConfiguration sourceConfiguration, ImportSettings destinationConfiguration)
		{
			_logger.LogInformation(
				"Checking if configurations allow using RelativitySync. SourceConfiguration.TypeOfExport: {typeOfExport}; DestinationConfiguration.ImageImport: {imageImport}; DestinationConfiguration.ProductionImport: {productionImport}",
				sourceConfiguration.TypeOfExport, 
				destinationConfiguration.ImageImport,
				destinationConfiguration.ProductionImport);

			return sourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch &&
			       !destinationConfiguration.ImageImport &&
			       !destinationConfiguration.ProductionImport;
		}

		private ProviderType GetProviderType(int sourceProviderId, int destinationProviderId)
		{
			_logger.LogInformation(
				$"Determining Integration Point provider type based on source and destination provider id's using {nameof(IProviderTypeService)} SourceProviderId: {{sourceProviderId}}; DestinationProviderId: {{destinationProviderId}}",
				sourceProviderId, 
				destinationProviderId);

			IProviderTypeService providerTypeService = null;
			try
			{
				providerTypeService = _agentLevelContainer.Value.Resolve<IProviderTypeService>();
				ProviderType providerType =
					providerTypeService.GetProviderType(sourceProviderId, destinationProviderId);
				_logger.LogInformation(
					"ProviderType for given providers has been determined as: {providerType}. SourceProviderId: {sourceProviderId}; DestinationProviderId: {destinationProviderId}",
					providerType, sourceProviderId, destinationProviderId);
				return providerType;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Getting Provider Type operation resulted in an error.");
				throw;
			}
			finally
			{
				if (providerTypeService != null)
				{
					_agentLevelContainer.Value.Release(providerTypeService);
				}
			}
		}

		private IntegrationPoint GetIntegrationPoint(int integrationPointId)
		{
			_logger.LogInformation("Retrieving Integration Point using IntegrationPointService. IntegrationPointId: {integrationPointId}", integrationPointId);

			IIntegrationPointService integrationPointService = null;
			try
			{
				integrationPointService = _agentLevelContainer.Value.Resolve<IIntegrationPointService>();
				IntegrationPoint integrationPoint = integrationPointService.GetRdo(integrationPointId);
				_logger.LogInformation("Integration Point with id: {integrationPointId} retrieved successfully.", integrationPointId);
				return integrationPoint;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Getting Integration Point operation resulted in an error.");
				throw;
			}
			finally
			{
				if (integrationPointService != null)
				{
					_agentLevelContainer.Value.Release(integrationPointService);
				}
			}
		}

		private bool IsRelativitySyncToggleEnabled()
		{
			_logger.LogInformation($"Checking if {nameof(EnableSyncToggle)} is enabled.");

			IToggleProvider toggleProvider = null;
			try
			{
				toggleProvider = _agentLevelContainer.Value.Resolve<IToggleProvider>();
				bool isEnabled = toggleProvider.IsEnabled<EnableSyncToggle>();
				_logger.LogInformation($"Confirmed that {nameof(EnableSyncToggle)} is {(isEnabled ? "enabled" : "disabled")}.");
				return isEnabled;

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Checking if {nameof(EnableSyncToggle)} is enabled resulted in an error.");
				throw;
			}
			finally
			{
				if (toggleProvider != null)
				{
					_agentLevelContainer.Value.Release(toggleProvider);
				}
			}
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