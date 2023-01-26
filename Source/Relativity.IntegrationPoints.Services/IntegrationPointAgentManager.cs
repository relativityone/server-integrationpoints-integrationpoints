using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Interfaces;
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

        private static string _METRIC_NAME = "IntegrationPoints.Performance.System";

        //Agent default container count based on Workload Discovery t-shirt response. Notes: 1. Global default (S:2, M:4, L:8, XL:12, XXL:16)
        private readonly List<WorkloadSizeDefinition> _workloadSizeDefaultSettings = new List<WorkloadSizeDefinition>()
        {
            new WorkloadSizeDefinition(minJobsCount: 0, maxJobsCount: 0, workloadSize: WorkloadSize.None),
            new WorkloadSizeDefinition(minJobsCount: 1, maxJobsCount: 1, workloadSize: WorkloadSize.One),
            new WorkloadSizeDefinition(minJobsCount: 2, maxJobsCount: 3, workloadSize: WorkloadSize.S),
            new WorkloadSizeDefinition(minJobsCount: 4, maxJobsCount: 7, workloadSize: WorkloadSize.M),
            new WorkloadSizeDefinition(minJobsCount: 8, maxJobsCount: int.MaxValue, workloadSize: WorkloadSize.L)
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
            Workload workload = new Workload()
            {
                Size = WorkloadSize.None
            };
            try
            {
                QueueInfo queueInfo = null;

                using (IWindsorContainer container = GetDependenciesContainer(workspaceArtifactId: -1))
                {
                    queueInfo = GetJobsFromQueue(container.Resolve<IQueueQueryManager>(), container.Resolve<IJobService>());

                    IInstanceSettingsManager instanceSettingManager = container.Resolve<IInstanceSettingsManager>();
                    List<WorkloadSizeDefinition> workloadSizeDefinitions = GetWorkloadSizeDefinitions(instanceSettingManager);
                    WorkloadSizeDefinition workloadSizeDefinition = SelectMatchingWorkloadSize(workloadSizeDefinitions, queueInfo.TotalWorkloadCount);
                    workload.Size = workloadSizeDefinition.WorkloadSize;
                }

                Logger.LogInformation("Selected workload size: {workloadSize}. QueueInfo: {@queueInfo}", workload.Size, queueInfo);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetWorkloadAsync_Error");
            }
            return Task.FromResult(workload);
        }

        private QueueInfo GetJobsFromQueue(IQueueQueryManager queueQueryManager, IJobService jobService)
        {
            QueueInfo queueInfo = new QueueInfo();

            List<Job> allQueueRecords = jobService.GetAllScheduledJobs().ToList();

            if (allQueueRecords.Any())
            {
                AgentTypeInformation agentInfo = GetAgentTypeInfo(queueQueryManager);

                List<Job> jobsReadyForProcessingByTimeCondition = allQueueRecords.Where(x => x.NextRunTime <= DateTime.UtcNow).ToList();
                List<long?> existingSyncWorkerTypeRootIds = jobsReadyForProcessingByTimeCondition.Where(x => x.TaskType == nameof(TaskType.SyncWorker)).Select(x => x.RootJobId).ToList();
                List<Job> jobsWithoutSyncEntityManagerWorkers = jobsReadyForProcessingByTimeCondition.Where(x => !(x.TaskType == nameof(TaskType.SyncEntityManagerWorker)
                                                                                                                    && existingSyncWorkerTypeRootIds.Contains(x.RootJobId))).ToList();

                queueInfo.TotalWorkloadCount = jobsWithoutSyncEntityManagerWorkers.Count();

                // Blocked jobs will not be removed from workload size because newly created Agent should remove them from queue.                
                queueInfo.BlockedJobsCount = allQueueRecords.Count(x => x.IsBlocked() || x.AgentTypeID != agentInfo.AgentTypeID);
                queueInfo.AllQueuedItemsCount = allQueueRecords.Count;
                queueInfo.JobsExcludedByTimeConditionCount = queueInfo.AllQueuedItemsCount - jobsReadyForProcessingByTimeCondition.Count;
                queueInfo.JobsExcludedBySyncWorkerPriorityCount = jobsReadyForProcessingByTimeCondition.Count - queueInfo.TotalWorkloadCount;

                SendWorkloadStateMetrics(queueInfo);
            }

            return queueInfo;
        }

        private AgentTypeInformation GetAgentTypeInfo(IQueueQueryManager queueQueryManager)
        {
            Guid agentGuid = Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);
            DataRow agentTypeInfo = queueQueryManager.GetAgentTypeInformation(agentGuid).Execute();
            if (agentTypeInfo == null)
            {
                throw new AgentNotFoundException($"Agent type information for GUID: {agentGuid} is unavailable");
            }
            return new AgentTypeInformation(agentTypeInfo);
        }

        private void SendWorkloadStateMetrics(QueueInfo queueInfo)
        {
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { "TotalWorkloadCount", queueInfo.TotalWorkloadCount },
                { "BlockedJobsCount", queueInfo.BlockedJobsCount },
                { "AllQueuedItemsCount", queueInfo.AllQueuedItemsCount },
                { "JobsExcludedByTimeConditionCount", queueInfo.JobsExcludedByTimeConditionCount },
                { "JobsExcludedBySyncWorkerPriorityCount", queueInfo.JobsExcludedBySyncWorkerPriorityCount }
            };

            Client.APMClient.CountOperation(_METRIC_NAME, correlationID: kCura.IntegrationPoints.Core.Constants.IntegrationPoints.Telemetry.WORKLOAD_METRICS_CORRELATION_ID_GUID,
                customData: data).Write();
        }

        private WorkloadSizeDefinition SelectMatchingWorkloadSize(List<WorkloadSizeDefinition> definitions, int pendingJobsCount)
        {
            Func<WorkloadSizeDefinition, bool> predicate = x => pendingJobsCount >= x.MinJobsCount && pendingJobsCount <= x.MaxJobsCount;

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