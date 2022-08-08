using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.RelativitySync.RipOverride
{
    internal sealed class SyncJobHistoryService : IJobHistoryService
    {
        private readonly IJobHistoryService _ordinaryJobHistoryService;

        public SyncJobHistoryService(IJobHistoryService ordinaryJobHistoryService)
        {
            _ordinaryJobHistoryService = ordinaryJobHistoryService;
        }

        public void UpdateRdoWithoutDocuments(JobHistory jobHistory)
        {
            // Method intentionally left empty.
            // The reason is to disable Synchronization step from updating job statuses
        }

        #region Proxy methods only

        public JobHistory GetOrCreateScheduledRunHistoryRdo(IntegrationPoint integrationPoint, Guid batchInstance, DateTime? startTimeUtc)
        {
            return _ordinaryJobHistoryService.GetOrCreateScheduledRunHistoryRdo(integrationPoint, batchInstance, startTimeUtc);
        }

        public JobHistory CreateRdo(IntegrationPoint integrationPoint, Guid batchInstance, ChoiceRef jobType, DateTime? startTimeUtc)
        {
            return _ordinaryJobHistoryService.CreateRdo(integrationPoint, batchInstance, jobType, startTimeUtc);
        }

        public IList<JobHistory> GetJobHistory(IList<int> jobHistoryArtifactIds)
        {
            return _ordinaryJobHistoryService.GetJobHistory(jobHistoryArtifactIds);
        }

        public JobHistory GetRdo(Guid batchInstance)
        {
            return _ordinaryJobHistoryService.GetRdo(batchInstance);
        }

        public JobHistory GetRdoWithoutDocuments(Guid batchInstance)
        {
            return _ordinaryJobHistoryService.GetRdoWithoutDocuments(batchInstance);
        }

        public void UpdateRdo(JobHistory jobHistory)
        {
            if (jobHistory.JobStatus.Guids[0] == JobStatusChoices.JobHistoryValidating.Guids[0])
            {
                //we're skipping job history update when setting validation status. it's done in Sync adapter
                return;
            }

            _ordinaryJobHistoryService.UpdateRdo(jobHistory);
        }

        public void DeleteRdo(int jobHistoryId)
        {
            _ordinaryJobHistoryService.DeleteRdo(jobHistoryId);
        }

        public IList<JobHistory> GetAll()
        {
            return _ordinaryJobHistoryService.GetAll();
        }

        #endregion
    }
}