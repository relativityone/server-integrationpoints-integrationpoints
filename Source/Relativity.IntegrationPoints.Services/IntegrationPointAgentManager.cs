﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Common.Metrics;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Managers;
using Newtonsoft.Json;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Installers;
using Relativity.Logging;
using Relativity.Telemetry.APM;
using WorkloadDiscovery;

namespace Relativity.IntegrationPoints.Services
{
	public class IntegrationPointAgentManager : KeplerServiceBase, IIntegrationPointsAgentManager
	{
		private Installer _installer;
		private static string _METRIC_NAME = "IntegrationPoints.Workloads.WorkloadSize";

		private readonly List<WorkloadSizeDefinition> _workloadSizeDefaultSettings = new List<WorkloadSizeDefinition>()
		{
            new WorkloadSizeDefinition(minJobsCount: 0, maxJobsCount: 0, workloadSize: WorkloadSize.None),
            new WorkloadSizeDefinition(minJobsCount: 1, maxJobsCount: 1, workloadSize: WorkloadSize.One),
            new WorkloadSizeDefinition(minJobsCount: 2, maxJobsCount: 3, workloadSize: WorkloadSize.S),
            new WorkloadSizeDefinition(minJobsCount: 4, maxJobsCount: 7, workloadSize: WorkloadSize.M),
            new WorkloadSizeDefinition(minJobsCount: 8, maxJobsCount: 23, workloadSize: WorkloadSize.L),
            new WorkloadSizeDefinition(minJobsCount: 24, maxJobsCount: 31, workloadSize: WorkloadSize.XL),
            new WorkloadSizeDefinition(minJobsCount: 32, maxJobsCount: int.MaxValue, workloadSize: WorkloadSize.XXL)
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
				int jobsCount = GetJobsFromQueue(container.Resolve<IQueueQueryManager>(), container.Resolve<IAPM>());

				IInstanceSettingsManager instanceSettingManager = container.Resolve<IInstanceSettingsManager>();
				List<WorkloadSizeDefinition> workloadSizeDefinitions = GetWorkloadSizeDefinitions(instanceSettingManager);
				
				WorkloadSizeDefinition workloadSizeDefinition = SelectMatchingWorkloadSize(workloadSizeDefinitions, jobsCount);
				
				Logger.LogInformation("Selected workload size: {workloadSize} for jobs count: {jobsCount}", workloadSizeDefinition.WorkloadSize, jobsCount);

				return Task.FromResult(new Workload()
				{
					Size = workloadSizeDefinition.WorkloadSize
				});
			}
		}		

		private int GetJobsFromQueue(IQueueQueryManager queueQueryManager, IAPM iApm)
        {
			DataRow queueItemsCount = queueQueryManager.GetJobsQueueDetails()
											.Execute()
											.AsEnumerable()
											.FirstOrDefault();
			
			int totalItems = queueItemsCount.Field<int>("Total");
			int blockedItems = queueItemsCount.Field<int>("Blocked");

			SendMetrics(iApm, totalItems, blockedItems);

			return totalItems;
        }

		private void SendMetrics(IAPM iApm, int totalItems, int blockedItems)
		{
			Dictionary<string, object> data = new Dictionary<string, object>()
				{					
					{ "TotalWorkloadCount", totalItems },
					{ "BlockedJobsCount", blockedItems }
				};

			iApm.CountOperation(_METRIC_NAME, customData: data).Write();
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