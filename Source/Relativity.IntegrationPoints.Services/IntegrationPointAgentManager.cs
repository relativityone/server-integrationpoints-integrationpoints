using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;
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
            Workload workload = new Workload()
            {
                Size = WorkloadSize.None
            };
            int totalJobsCount = 0;
            int blockedJobsCount = 0;
            try
            {
                using (IWindsorContainer container = GetDependenciesContainer(workspaceArtifactId: -1))
                {
                    (totalJobsCount, blockedJobsCount) = GetJobsFromQueue(container.Resolve<IQueueQueryManager>());

                    IInstanceSettingsManager instanceSettingManager = container.Resolve<IInstanceSettingsManager>();
                    List<WorkloadSizeDefinition> workloadSizeDefinitions = GetWorkloadSizeDefinitions(instanceSettingManager);
                    WorkloadSizeDefinition workloadSizeDefinition = SelectMatchingWorkloadSize(workloadSizeDefinitions, totalJobsCount);
                    workload.Size = workloadSizeDefinition.WorkloadSize;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetWorkloadAsync_Error");
            }
            Logger.LogInformation("Selected workload size: {workloadSize} for jobs count: {jobsCount} - blockedJobsCount: {blockedJobsCount}",
                workload.Size, totalJobsCount, blockedJobsCount);
            return Task.FromResult(workload);
        }

        private (int TotalJobsCount, int BlockedJobsCount) GetJobsFromQueue(IQueueQueryManager queueQueryManager)
        {
            int totalItems = 0;
            int blockedItems = 0;         
            
            DataTable queryResult = queueQueryManager.GetAllJobs().Execute();

            if(queryResult != null)
            {
                AgentTypeInformation agentInfo = GetAgentTypeInfo(queueQueryManager);
                
                IEnumerable<Job> allJobs = queryResult.Rows.Cast<DataRow>().Select(row => new Job(row));
                IEnumerable<Job> jobsReadyForProcessingByTimeCondition = allJobs.Where(x => x.NextRunTime <= DateTime.UtcNow);
                IEnumerable<long?> existingSyncWorkerTypeRootIds = jobsReadyForProcessingByTimeCondition.Where(x => x.TaskType == JobTaskTypeNames.SYNC_WORKER).Select(x => x.RootJobId);
                IEnumerable<Job> jobsWithoutSyncEntityManagerWorkers = jobsReadyForProcessingByTimeCondition.Where(x => !(x.TaskType == JobTaskTypeNames.SYNC_ENTITY_WORKER_MANAGER
                                                                                                                    && existingSyncWorkerTypeRootIds.Contains(x.RootJobId)));     
               
                totalItems = jobsWithoutSyncEntityManagerWorkers.Count();

                // Blocked jobs will not be removed from workload size because newly created Agent should remove them from queue.                
                blockedItems = allJobs.Where(x => (x.StopState != StopState.None && x.StopState != StopState.DrainStopped && x.LockedByAgentID == null) 
                                                        || x.AgentTypeID != agentInfo.AgentTypeID).Count();

                int queueCount = allJobs.Count();
                int jobsExcludedByTimeConditionCount = queueCount - jobsReadyForProcessingByTimeCondition.Count();
                int jobBlockedBySyncWorkerPriorityCount = jobsReadyForProcessingByTimeCondition.Count() - totalItems;

                SendWorkloadStateMetrics(totalItems, 
                    blockedItems,
                    queueCount,
                    jobsExcludedByTimeConditionCount,
                    jobBlockedBySyncWorkerPriorityCount);
            }
            return (totalItems, blockedItems);
        }

        private AgentTypeInformation GetAgentTypeInfo(IQueueQueryManager queueQueryManager)
        {
            Guid agentGuid = Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);
            DataRow agentTypeInfo = queueQueryManager.GetAgentTypeInformation(agentGuid).Execute();
            if(agentTypeInfo == null)
            {
                throw new AgentNotFoundException($"Agent type information for GUID: {agentGuid} is unavailable");
            }
            return new AgentTypeInformation(agentTypeInfo);
        }

        private void SendWorkloadStateMetrics(int totalItems, int blockedItems, int queueCount, int jobsExcludedByTimeConditionCount, int jobBlockedBySyncWorkerPriorityCount)
        {
            Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "TotalWorkloadCount", totalItems },
                    { "BlockedJobsCount", blockedItems },
                    { "AllQueuedItemsCount", queueCount },
                    { "JobsExcludedByTimeConditionCount", jobsExcludedByTimeConditionCount },
                    { "JobsExcludedBySyncWorkerPriorityCount", jobBlockedBySyncWorkerPriorityCount }
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