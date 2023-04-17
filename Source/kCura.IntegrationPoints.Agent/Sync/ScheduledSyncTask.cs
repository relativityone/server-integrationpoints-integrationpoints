using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.Sync
{
    internal class ScheduledSyncTask : IScheduledSyncTask
    {
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IJobHistoryErrorService _jobHistoryErrorService;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly ITaskParameterHelper _taskParameterHelper;
        private readonly IDateTime _dateTime;

        public ScheduledSyncTask(
            IJobHistoryService jobHistoryService,
            IJobHistoryErrorService jobHistoryErrorService,
            IIntegrationPointService integrationPointService,
            ITaskParameterHelper taskParameterHelper,
            IDateTime dateTime)
        {
            _jobHistoryService = jobHistoryService;
            _jobHistoryErrorService = jobHistoryErrorService;
            _integrationPointService = integrationPointService;
            _taskParameterHelper = taskParameterHelper;
            _dateTime = dateTime;
        }

        public void Execute(Job job)
        {
            ExecuteAsync(job).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(Job job)
        {
            int jobHistoryId = await RequireJobHistoryAsync(job).ConfigureAwait(false);
            try
            {
                throw new NotImplementedException("This code path should not be reached. Contact Customer Support for help.");
            }
            catch (Exception ex)
            {
                await _jobHistoryService.UpdateStatusAsync(
                    job.WorkspaceID, jobHistoryId, JobStatusChoices.JobHistoryErrorJobFailedGuid)
                .ConfigureAwait(false);

                await _jobHistoryErrorService.AddJobErrorAsync(job.WorkspaceID, jobHistoryId, ex).ConfigureAwait(false);
            }
        }

        private async Task<int> RequireJobHistoryAsync(Job job)
        {
            Guid batchInstanceId = _taskParameterHelper.GetBatchInstance(job);
            JobHistory jobHistory = await _jobHistoryService.ReadJobHistoryByGuidAsync(job.WorkspaceID, batchInstanceId).ConfigureAwait(false);
            if (jobHistory == null)
            {
                IntegrationPointSlimDto integrationPoint = _integrationPointService.ReadSlim(job.RelatedObjectArtifactID);

                return await _jobHistoryService.CreateJobHistoryAsync(job.WorkspaceID, new JobHistory
                {
                    Name = integrationPoint.Name,
                    IntegrationPoint = new[] { integrationPoint.ArtifactId },
                    BatchInstance = batchInstanceId.ToString(),
                    JobType = JobTypeChoices.JobHistoryScheduledRun,
                    JobStatus = JobStatusChoices.JobHistoryPending,
                    Overwrite = integrationPoint.SelectedOverwrite,
                    JobID = job.JobId.ToString(),
                    StartTimeUTC = _dateTime.UtcNow
                });
            }

            return jobHistory.ArtifactId;
        }
    }
}
