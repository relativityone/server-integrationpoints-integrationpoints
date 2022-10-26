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

        private int _workspaceId;
        private Guid _importJobId;

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

        public async Task<IDisposable> AttachAsync(int workspaceId, Guid importJobId)
        {
            try
            {
                _workspaceId = workspaceId;
                _importJobId = importJobId;

                TimeSpan progressUpdatePeriod = await _instanceSettings.GetSyncProgressUpdatePeriodAsync(_DEFAULT_PROGRESS_UPDATE_PERIOD).ConfigureAwait(false);

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

        private async Task HandleProgressAsync()
        {
            try
            {
                ImportProgress importProgress = await GetImportJobProgressAsync().ConfigureAwait(false);

                await _progressUpdater.UpdateJobProgressAsync(
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
                        _workspaceId,
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
