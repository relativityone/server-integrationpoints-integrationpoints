using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.InstanceSettings;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.Import.V1.Models;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress
{
    internal class JobProgressHandler : IJobProgressHandler
    {
        private readonly IImportApiService _importApiService;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly ITimerFactory _timerFactory;
        private readonly IInstanceSettings _instanceSettings;
        private readonly IAPILog _logger;

        public JobProgressHandler(IImportApiService importApiService, IJobHistoryService jobHistoryService, ITimerFactory timerFactory, IInstanceSettings instanceSettings, IAPILog logger)
        {
            _importApiService = importApiService;
            _jobHistoryService = jobHistoryService;
            _timerFactory = timerFactory;
            _instanceSettings = instanceSettings;
            _logger = logger.ForContext<JobProgressHandler>();
        }

        public async Task<IDisposable> BeginUpdateAsync(ImportJobContext importJobContext)
        {
            TimeSpan interval = await _instanceSettings.GetCustomProviderProgressUpdateIntervalAsync()
                .ConfigureAwait(false);

            _logger.LogInformation("Progress update interval: {interval}", interval);

            ITimer timer = _timerFactory.Create(async (state) => await UpdateProgressAsync(importJobContext).ConfigureAwait(false), null, TimeSpan.Zero, interval, "CustomProviderProgressUpdateTimer");
            return timer;
        }

        public async Task UpdateProgressAsync(ImportJobContext importJobContext)
        {
            try
            {
                ImportProgress progress = await _importApiService.GetJobImportProgressValueAsync(importJobContext).ConfigureAwait(false);

                await _jobHistoryService
                    .UpdateProgressAsync(importJobContext.WorkspaceId, importJobContext.JobHistoryId, progress.ImportedRecords, progress.ErroredRecords)
                    .ConfigureAwait(false);

                _logger.LogInformation("Progress has been updated. Imported items count: {importedItemsCount} Failed items count: {failedItemsCount}", progress.ImportedRecords, progress.ErroredRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update job progress for Job History ID: {jobHistoryId}", importJobContext.JobHistoryId);
            }
        }

        public async Task WaitForJobToFinish(ImportJobContext importJobContext, CompositeCancellationToken token)
        {
            TimeSpan interval = TimeSpan.FromSeconds(5);

            ImportDetails result;
            ImportState state = ImportState.Unknown;
            do
            {
                if (token.IsStopRequested)
                {
                    await _importApiService.CancelJobAsync(importJobContext).ConfigureAwait(false);
                }

                if (token.IsDrainStopRequested)
                {
                    return;
                }

                await Task.Delay(interval).ConfigureAwait(false);

                result = await _importApiService.GetJobImportStatusAsync(importJobContext).ConfigureAwait(false);
                if (result.State != state)
                {
                    state = result.State;
                    _logger.LogInformation("Import status: {@status}", result);
                }
            }
            while (!result.IsFinished);
        }

        public async Task UpdateReadItemsCountAsync(Job job, CustomProviderJobDetails jobDetails)
        {
            int readItemsCount = jobDetails
                .Batches
                .Where(x => x.IsAddedToImportQueue)
                .Sum(x => x.NumberOfRecords);

            await _jobHistoryService
                .UpdateReadItemsCountAsync(job.WorkspaceID, jobDetails.JobHistoryID, readItemsCount)
                .ConfigureAwait(false);
        }

        public async Task SetTotalItemsAsync(int workspaceId, int jobHistoryId, int totalItemsCount)
        {
            await _jobHistoryService.SetTotalItemsAsync(workspaceId, jobHistoryId, totalItemsCount).ConfigureAwait(false);
        }
    }
}
