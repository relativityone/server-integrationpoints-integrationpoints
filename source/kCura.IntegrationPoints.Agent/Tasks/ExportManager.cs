using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
    public class ExportManager : SyncManager
    {
        public ExportManager(ICaseServiceContext caseServiceContext, IDataProviderFactory providerFactory, IJobManager jobManager, IJobService jobService, IHelper helper, IIntegrationPointService integrationPointService, ISerializer serializer, IGuidService guidService, IJobHistoryService jobHistoryService, JobHistoryErrorService jobHistoryErrorService, IScheduleRuleFactory scheduleRuleFactory, IEnumerable<IBatchStatus> batchStatuses) : base(caseServiceContext, providerFactory, jobManager, jobService, helper, integrationPointService, serializer, guidService, jobHistoryService, jobHistoryErrorService, scheduleRuleFactory, batchStatuses)
        {
        }

        protected override TaskType GetTaskType()
        {
            return TaskType.ExportWorker;
        }

		public override int BatchSize
		{
			get
			{
				//Currently Export Shared library (kCura.WinEDDS) is making usage of batching internalLy
				//so for now we need to create only one worker job
				//Instead of overriding GetUnbatchedIDs we change BatchSize
				//Overriding GetUnbatchedIDs would result in incorrect "Items Imported" property value (1 instead of document count)
				return int.MaxValue;
			}
		}
	}
}
