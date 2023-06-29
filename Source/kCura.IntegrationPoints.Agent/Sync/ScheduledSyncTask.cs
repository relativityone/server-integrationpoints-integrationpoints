using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.RelativitySync;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Utils;
using Relativity.API;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.Interfaces.Workspace.Models;

namespace kCura.IntegrationPoints.Agent.Sync
{
    internal class ScheduledSyncTask : IScheduledSyncTask
    {
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IJobHistoryErrorService _jobHistoryErrorService;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly ITaskParameterHelper _taskParameterHelper;
        private readonly IDateTime _dateTime;
        private readonly IRelativitySyncAppIntegration _syncAppIntegration;
        private readonly IKeplerServiceFactory _keplerService;

        private readonly IAPILog _log;

        public ScheduledSyncTask(
            IJobHistoryService jobHistoryService,
            IJobHistoryErrorService jobHistoryErrorService,
            IIntegrationPointService integrationPointService,
            ITaskParameterHelper taskParameterHelper,
            IDateTime dateTime,
            IRelativitySyncAppIntegration syncAppIntegration,
            IKeplerServiceFactory keplerService,
            IAPILog log)
        {
            _jobHistoryService = jobHistoryService;
            _jobHistoryErrorService = jobHistoryErrorService;
            _integrationPointService = integrationPointService;
            _taskParameterHelper = taskParameterHelper;
            _dateTime = dateTime;
            _syncAppIntegration = syncAppIntegration;
            _log = log;
            _keplerService = keplerService;
        }

        public void Execute(Job job)
        {
            ExecuteAsync(job).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(Job job)
        {
            IntegrationPointDto integrationPoint = _integrationPointService.Read(job.RelatedObjectArtifactID);

            int jobHistoryId = await RequireJobHistoryAsync(job, integrationPoint).ConfigureAwait(false);
            try
            {
                _log.LogInformation("Submitting Scheduled Sync Job for JobHistory {jobHistoryId} in IntegrationPoint {integrationPointId}", jobHistoryId, integrationPoint.ArtifactId);
                await _syncAppIntegration.SubmitSyncJobAsync(job.WorkspaceID, integrationPoint, jobHistoryId, job.SubmittedBy).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Exception ocurred during Sync Job submit.");
                await _jobHistoryService.UpdateStatusAsync(
                        job.WorkspaceID, jobHistoryId, JobStatusChoices.JobHistoryErrorJobFailedGuid)
                    .ConfigureAwait(false);

                await _jobHistoryErrorService.AddJobErrorAsync(job.WorkspaceID, jobHistoryId, ex).ConfigureAwait(false);
            }
        }

        private async Task<int> RequireJobHistoryAsync(Job job, IntegrationPointDto integrationPoint)
        {
            Guid batchInstanceId = _taskParameterHelper.GetBatchInstance(job);

            int? jobHistoryId = await ReadJobHistoryAsync(job.WorkspaceID, batchInstanceId).ConfigureAwait(false)
                ?? await CreateScheduledJobHistoryAsync(job.WorkspaceID, batchInstanceId, integrationPoint).ConfigureAwait(false);

            return jobHistoryId.Value;
        }

        private async Task<int?> ReadJobHistoryAsync(int workspaceId, Guid batchInstanceId)
        {
            JobHistory jobHistory = await _jobHistoryService.ReadJobHistoryByGuidAsync(workspaceId, batchInstanceId).ConfigureAwait(false);
            if (jobHistory != null)
            {
                _log.LogInformation("JobHistory for BatchInstanceId {batchInstanceId} was read - {jobHistoryId}.", batchInstanceId, jobHistory.ArtifactId);

                return jobHistory.ArtifactId;
            }

            return null;
        }

        private async Task<int?> CreateScheduledJobHistoryAsync(int workspaceId, Guid batchInstanceId, IntegrationPointDto integrationPoint)
        {
            string destinationWorkspaceName = await FormatDestinationWorkspaceName(
                    integrationPoint.DestinationConfiguration.CaseArtifactId)
                .ConfigureAwait(false);

            int jobHistoryId = await _jobHistoryService.CreateJobHistoryAsync(workspaceId, new JobHistory
            {
                Name = integrationPoint.Name,
                IntegrationPoint = new[] { integrationPoint.ArtifactId },
                BatchInstance = batchInstanceId.ToString(),
                JobType = JobTypeChoices.JobHistoryScheduledRun,
                JobStatus = JobStatusChoices.JobHistoryPending,
                Overwrite = integrationPoint.SelectedOverwrite,
                DestinationInstance = FederatedInstanceManager.LocalInstance.Name,
                DestinationWorkspace = destinationWorkspaceName,
                StartTimeUTC = _dateTime.UtcNow
            });

            _log.LogInformation("JobHistory for BatchInstanceId {batchInstanceId} was created - {jobHistoryId}.", batchInstanceId, jobHistoryId);

            return jobHistoryId;
        }

        private async Task<string> FormatDestinationWorkspaceName(int destinationWorkspaceId)
        {
            using (IWorkspaceManager workspaceManager = await _keplerService.CreateProxyAsync<IWorkspaceManager>().ConfigureAwait(false))
            {
                WorkspaceResponse workspace = await workspaceManager.ReadAsync(destinationWorkspaceId).ConfigureAwait(false);

                return WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(workspace.Name, workspace.ArtifactID);
            }
        }
    }
}
