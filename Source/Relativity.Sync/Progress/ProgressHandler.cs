using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Services;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Progress
{
    internal class ProgressHandler : IProgressHandler
    {
        private readonly ITimerFactory _timerFactory;
        private readonly ISourceServiceFactoryForAdmin _serviceFactory;
        private readonly IInstanceSettings _instanceSettings;
        private readonly IJobProgressUpdater _progressUpdater;
        private readonly IBatchRepository _batchRepository;
        private readonly IAPILog _log;

        private readonly TimeSpan _DEFAULT_PROGRESS_UPDATE_PERIOD = TimeSpan.FromSeconds(10);

        private int _sourceWorkspaceId;
        private int _destinationWorkspaceId;
        private Guid _importJobId;
        private int _jobHistoryId;
        private int _syncConfigurationArtifactId;
        private bool _isRunning;

        private int _readDocumentsCountCache;
        private int _failedReadDocumentsCountCache;
        private List<int> _batchesArtifactIds;

        public ProgressHandler(
            ITimerFactory timerFactory,
            ISourceServiceFactoryForAdmin serviceFactory,
            IInstanceSettings instanceSettings,
            IJobProgressUpdater progressUpdater,
            IBatchRepository batchRepository,
            IAPILog log)
        {
            _timerFactory = timerFactory;
            _serviceFactory = serviceFactory;
            _instanceSettings = instanceSettings;
            _log = log;
            _progressUpdater = progressUpdater;
            _batchRepository = batchRepository;
            _isRunning = false;
        }

        public async Task<IDisposable> AttachAsync(int sourceWorkspaceId, int destinationWorkspaceId, int jobHistoryId, Guid importJobId, int syncConfigurationArtifactId)
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
                _syncConfigurationArtifactId = syncConfigurationArtifactId;

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
            if (_isRunning)
            {
                return;
            }

            try
            {
                _isRunning = true;
                Progress batchesProgress = await GetBatchesProgressAsync().ConfigureAwait(false);
                Progress importJobProgress = await GetImportJobProgressAsync().ConfigureAwait(false);
                Progress progress = batchesProgress + importJobProgress;
                await _progressUpdater.UpdateJobProgressAsync(_sourceWorkspaceId, _jobHistoryId, progress).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Sync Job progress update failed. Progress won't be updated in this cycle.");
            }
            finally
            {
                _isRunning = false;
            }
        }

        private async Task<Progress> GetBatchesProgressAsync()
        {
            if (_batchesArtifactIds == null)
            {
                _batchesArtifactIds = (await _batchRepository
                        .GetAllAsync(_sourceWorkspaceId, _syncConfigurationArtifactId, _importJobId)
                        .ConfigureAwait(false))
                    .Select(x => x.ArtifactId)
                    .ToList();

                if (!_batchesArtifactIds.Any())
                {
                    _log.LogWarning("Batches not found while progress handling.");
                }
            }

            IEnumerable<IBatch> batches = await _batchRepository.GetBatchesWithIdsAsync(
                    _sourceWorkspaceId,
                    _syncConfigurationArtifactId,
                    _batchesArtifactIds,
                    _importJobId)
                .ConfigureAwait(false);

            int readDocumentsCount = 0;
            int failedReadDocumentsCount = 0;
            int readDocumentsCountCache = _readDocumentsCountCache;
            int failedReadDocumentsCountCache = _failedReadDocumentsCountCache;
            foreach (IBatch batch in batches)
            {
                readDocumentsCount += batch.ReadDocumentsCount;
                failedReadDocumentsCount += batch.FailedReadDocumentsCount;
                if (batch.IsFinished || batch.Status == BatchStatus.Generated)
                {
                    readDocumentsCountCache += batch.ReadDocumentsCount;
                    failedReadDocumentsCountCache += batch.FailedReadDocumentsCount;
                    _batchesArtifactIds.Remove(batch.ArtifactId);
                }
            }
            int completedRecordsCount = _readDocumentsCountCache + readDocumentsCount;
            int failedRecordsCount = _failedReadDocumentsCountCache + failedReadDocumentsCount;

            _readDocumentsCountCache = readDocumentsCountCache;
            _failedReadDocumentsCountCache = failedReadDocumentsCountCache;

            return new Progress(completedRecordsCount, failedRecordsCount, 0);
        }

        private async Task<Progress> GetImportJobProgressAsync()
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

                return new Progress(0, importProgress.Value.ErroredRecords, importProgress.Value.ImportedRecords);
            }
        }
    }
}
