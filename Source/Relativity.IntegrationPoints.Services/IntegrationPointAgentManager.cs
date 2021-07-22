using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core.Data;
using Newtonsoft.Json;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Installers;
using Relativity.Logging;
using WorkloadDiscovery;

namespace Relativity.IntegrationPoints.Services
{
	public class IntegrationPointAgentManager : KeplerServiceBase, IIntegrationPointsAgentManager
	{
		private Installer _installer;

		private readonly List<WorkloadSizeDefinition> _workloadSizeDefaultSettings = new List<WorkloadSizeDefinition>()
		{
			new WorkloadSizeDefinition(minJobsCount: 0, maxJobsCount: 0, workloadSize: WorkloadSize.None),
			new WorkloadSizeDefinition(minJobsCount: 1, maxJobsCount: 1, workloadSize: WorkloadSize.One),
			new WorkloadSizeDefinition(minJobsCount: 2, maxJobsCount: 2, workloadSize: WorkloadSize.S),
			new WorkloadSizeDefinition(minJobsCount: 3, maxJobsCount: 4, workloadSize: WorkloadSize.M),
			new WorkloadSizeDefinition(minJobsCount: 5, maxJobsCount: int.MaxValue, workloadSize: WorkloadSize.L),
		};

		protected override Installer Installer => _installer ?? (_installer = new IntegrationPointAgentManagerInstaller());

		public IntegrationPointAgentManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
			: base(logger, permissionRepositoryFactory, container)
		{
		}

		public IntegrationPointAgentManager(ILog logger)
			: base(logger)
		{
		}

		public Task<Workload> GetWorkloadAsync()
		{
			using (IWindsorContainer container = GetDependenciesContainer(workspaceArtifactId: -1))
			{
				IQueueQueryManager queueQueryManager = container.Resolve<IQueueQueryManager>();
				int pendingJobsCount = queueQueryManager.GetPendingJobsCount().Execute();

				IInstanceSettingsManager instanceSettingManager = container.Resolve<IInstanceSettingsManager>();
				List<WorkloadSizeDefinition> workloadSizeDefinitions = GetWorkloadSizeDefinitions(instanceSettingManager);
				
				WorkloadSizeDefinition workloadSizeDefinition = SelectMatchingWorkloadSize(workloadSizeDefinitions, pendingJobsCount);
				
				Logger.LogInformation("Selected workload size: {size} for jobs count: {count}", workloadSizeDefinition.WorkloadSize, pendingJobsCount);

				return Task.FromResult(new Workload()
				{
					Size = workloadSizeDefinition.WorkloadSize
				});
			}
		}

		private WorkloadSizeDefinition SelectMatchingWorkloadSize(List<WorkloadSizeDefinition> definitions, int pendingJobsCount)
		{
			Func<WorkloadSizeDefinition, bool> predicate =  x => pendingJobsCount >= x.MinJobsCount && pendingJobsCount <= x.MaxJobsCount;

			WorkloadSizeDefinition workloadSizeDefinition = definitions.FirstOrDefault(predicate);

			if (workloadSizeDefinition == null)
			{
				Logger.LogWarning("Could not match workload size definition to pending jobs count: {count}. Default values will be used.", pendingJobsCount);
				workloadSizeDefinition = _workloadSizeDefaultSettings.First(predicate);
			}

			return workloadSizeDefinition;
		}

		private List<WorkloadSizeDefinition> GetWorkloadSizeDefinitions(IInstanceSettingsManager instanceSettingsManager)
		{
			List<WorkloadSizeDefinition> workloadSizeDefinitions = _workloadSizeDefaultSettings;

			try
			{
				string workloadSizeSettings = instanceSettingsManager.GetWorkloadSizeSettings();
				
				if (!string.IsNullOrWhiteSpace(workloadSizeSettings))
				{
					Logger.LogInformation("Workload size Instance Setting value: {value}", workloadSizeSettings);
					workloadSizeDefinitions = JsonConvert.DeserializeObject<List<WorkloadSizeDefinition>>(workloadSizeSettings);
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Exception occurred when retrieving workload size settings from Instance Settings. Default values will be used.");
			}

			return workloadSizeDefinitions;
		}
		
		public void Dispose()
		{
		}

		internal class WorkloadSizeDefinition
		{
			public WorkloadSizeDefinition()
			{
			}

			public WorkloadSizeDefinition(int minJobsCount, int maxJobsCount, WorkloadSize workloadSize)
			{
				MinJobsCount = minJobsCount;
				MaxJobsCount = maxJobsCount;
				WorkloadSize = workloadSize;
			}

			public int MinJobsCount { get; set; }
			public int MaxJobsCount { get; set; }
			public WorkloadSize WorkloadSize { get; set; }
		}
	}
}