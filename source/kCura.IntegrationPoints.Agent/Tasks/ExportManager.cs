using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		/// <summary>
		/// This method returns record (batch) ids that should be processed by ExportWorker class
		/// </summary>
		/// <param name="job">Details of the export job</param>
		/// <returns>List of batch ids to be processed</returns>
	    public override IEnumerable<string> GetUnbatchedIDs(Job job)
		{
			//Currently Export Shared library (kCura.WinEDDS) is making usage of batching internalLy
			//so for now we need to create only one worker job
			yield return job.JobId.ToString();
		}

	}
}
