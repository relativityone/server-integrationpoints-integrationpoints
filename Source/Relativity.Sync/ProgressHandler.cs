using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Services;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Utils;

namespace Relativity.Sync
{
    internal class ProgressHandler : IProgressHandler
    {
        private readonly ITimerFactory _timerFactory;
        private readonly ISourceServiceFactoryForAdmin _serviceFactory;
        private readonly IInstanceSettings _instanceSettings;
        private readonly IJobProgressUpdater _progressUpdater;
        private readonly IAPILog _log;

        private readonly TimeSpan _DEFAULT_PROGRESS_UPDATE_PERIOD = TimeSpan.FromSeconds(10);

        private int _sourceWorkspaceId;
        private int _destinationWorkspaceId;
        private Guid _importJobId;
        private int _jobHistoryId;

        public ProgressHandler(
            ITimerFactory timerFactory,
            ISourceServiceFactoryForAdmin serviceFactory,
            IInstanceSettings instanceSettings,
            IJobProgressUpdater progressUpdater,
            IAPILog log)
        {
            _timerFactory = timerFactory;
            _serviceFactory = serviceFactory;
            _instanceSettings = instanceSettings;
            _log = log;
            _progressUpdater = progressUpdater;
        }

        public async Task<IDisposable> AttachAsync(int sourceWorkspaceId, int destinationWorkspaceId, int jobHistoryId, Guid importJobId)
        {
            try
            {
                _log.LogInformation(
                    "Creating Progress Timer - " +
                    "SourceWorkspaceId: {sourceWorkspaceId}, DestinationWorkspaceId: {destinationWorkspaceId}, " +
                    "JobHistoryId: {jobHistoryId}, ImportJobId: {importJobId}",
                    sourceWorkspaceId,
                    destinationWorkspaceId,
                    jobHistoryId,
                    importJobId);

                _sourceWorkspaceId = sourceWorkspaceId;
                _destinationWorkspaceId = destinationWorkspaceId;
                _importJobId = importJobId;
                _jobHistoryId = jobHistoryId;

                TimeSpan progressUpdatePeriod = await _instanceSettings.GetSyncProgressUpdatePeriodAsync(_DEFAULT_PROGRESS_UPDATE_PERIOD).ConfigureAwait(false);

                _log.LogInformation("Progress will be updated every {period}", progressUpdatePeriod);

                ITimer timer = _timerFactory.Create();
                timer.Activate((state) => HandleProgressAsync().GetAwaiter().GetResult(), null, TimeSpan.Zero, progressUpdatePeriod);

                return timer;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Progress Timer creation failed. Progress won't be reported across the Sync Job.");
                return Disposable.Empty;
            }
        }

        public async Task HandleProgressAsync()
        {
            try
            {
                _log.LogInformation("Updating Prgroess...");

                ImportProgress importProgress = await GetImportJobProgressAsync().ConfigureAwait(false);

                _log.LogInformation(
                    "Import Job Progress - ImportedRecords: {importedRecords}, ErroredRecords: {erroredRecords}",
                    importProgress.ImportedRecords,
                    importProgress.ErroredRecords);

                await _progressUpdater.UpdateJobProgressAsync(
                        _sourceWorkspaceId,
                        _jobHistoryId,
                        importProgress.ImportedRecords,
                        importProgress.ErroredRecords)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Sync Job progress update failed. Progress won't be updated in this cycle.");
                return;
            }
        }

        private async Task<ImportProgress> GetImportJobProgressAsync()
        {
            using (IImportJobController job = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                ValueResponse<ImportProgress> importProgress = await job.GetProgressAsync(
                        _destinationWorkspaceId,
                        _importJobId)
                    .ConfigureAwait(false);

                if (!importProgress.IsSuccess)
                {
                    throw new Exception(
                        "Reading progress for IAPI Job failed. Progress won't be updated in this cycle. " +
                        $"Error: {importProgress.ErrorCode}:{importProgress.ErrorMessage}");
                }

                return importProgress.Value;
            }
        }
    }
}
