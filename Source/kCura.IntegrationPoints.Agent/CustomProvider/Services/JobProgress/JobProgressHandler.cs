using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.InstanceSettings;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Utils;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Kepler;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Services;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress
{
    public class JobProgressHandler : IJobProgressHandler
    {
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly ITimerFactory _timerFactory;
        private readonly IInstanceSettings _instanceSettings;
        private readonly IAPILog _logger;

        public JobProgressHandler(IKeplerServiceFactory serviceFactory, IJobHistoryService jobHistoryService, ITimerFactory timerFactory, IInstanceSettings instanceSettings, IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _jobHistoryService = jobHistoryService;
            _timerFactory = timerFactory;
            _instanceSettings = instanceSettings;
            _logger = logger.ForContext<JobProgressHandler>();
        }

        public async Task<IDisposable> BeginUpdateAsync(int workspaceId, Guid importJobId, int jobHistoryId)
        {
            TimeSpan interval = await _instanceSettings.GetCustomProviderProgressUpdateIntervalAsync()
                .ConfigureAwait(false);

            _logger.LogInformation("Progress update interval: {interval}", interval);

            ITimer timer = _timerFactory.Create(async (state) => await UpdateProgressAsync(workspaceId, importJobId, jobHistoryId).ConfigureAwait(false), null, TimeSpan.Zero, interval, "CustomProviderProgressUpdateTimer");
            return timer;
        }

        private async Task UpdateProgressAsync(int workspaceId, Guid importJobId, int jobHistoryId)
        {
            try
            {
                Progress importJobProgress = await GetImportJobProgressAsync(workspaceId, importJobId).ConfigureAwait(false);

                await _jobHistoryService.UpdateProgressAsync(workspaceId, jobHistoryId, importJobProgress.TransferredDocumentsCount, importJobProgress.FailedDocumentsCount)
                    .ConfigureAwait(false);

                _logger.LogInformation("Progress has been updated. Imported items count: {importedItemsCount} Failed items count: {failedItemsCount}", importJobProgress.TransferredDocumentsCount, importJobProgress.FailedDocumentsCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update job progress for Job History ID: {jobHistoryId}", jobHistoryId);
            }
        }

        private async Task<Progress> GetImportJobProgressAsync(int workspaceId, Guid importJobId)
        {
            try
            {
                using (IImportJobController jobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
                {
                    ValueResponse<ImportProgress> response = await jobController.GetProgressAsync(workspaceId, importJobId).ConfigureAwait(false);
                    ImportProgress progress = response.UnwrapOrThrow();
                    return new Progress(progress.ErroredRecords, progress.ImportedRecords);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get import job progress for job ID: {importJobId}", importJobId);
                throw;
            }
        }
    }
}
