using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1.Models;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Progress
{
    internal class ProgressHandler : IProgressHandler
    {
        private readonly ITimerFactory _timerFactory;
        private readonly IInstanceSettings _instanceSettings;
        private readonly IJobProgressUpdater _progressUpdater;
        private readonly IBatchRepository _batchRepository;
        private readonly IImportService _importService;
        private readonly IAPILog _log;

        private readonly TimeSpan _DEFAULT_PROGRESS_UPDATE_PERIOD = TimeSpan.FromSeconds(10);

        private int _sourceWorkspaceId;
        private Guid _importJobId;
        private int _jobHistoryId;
        private int _syncConfigurationArtifactId;
        private bool _isRunning;

        private int _readDocumentsCountCache;
        private int _failedReadDocumentsCountCache;
        private List<int> _batchesArtifactIds;

        public ProgressHandler(
            ITimerFactory timerFactory,
            IInstanceSettings instanceSettings,
            IJobProgressUpdater progressUpdater,
            IBatchRepository batchRepository,
            IImportService importService,
            IAPILog log)
        {
            _timerFactory = timerFactory;
            _instanceSettings = instanceSettings;
            _log = log;
            _progressUpdater = progressUpdater;
            _batchRepository = batchRepository;
            _importService = importService;
            _isRunning = false;
        }

        public async Task<IDisposable> AttachAsync(
            int sourceWorkspaceId,
            int destinationWorkspaceId,
            int jobHistoryId,
            Guid importJobId,
            int syncConfigurationArtifactId,
            IEnumerable<int> batchIds)
        {
            try
            {
                Initialize(sourceWorkspaceId, destinationWorkspaceId, jobHistoryId, importJobId, syncConfigurationArtifactId, batchIds);

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
                _log.LogInformation("Progress update is already running...");
                return;
            }

            try
            {
                _isRunning = true;
                Progress batchesProgress = await GetBatchesProgressAsync().ConfigureAwait(false);
                Progress importJobProgress = await GetImportJobProgressAsync().ConfigureAwait(false);
                Progress progress = batchesProgress + importJobProgress;

                _log.LogInformation("Updating Progress with {@progress}", progress);

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

        private void Initialize(
            int sourceWorkspaceId,
            int destinationWorkspaceId,
            int jobHistoryId,
            Guid importJobId,
            int syncConfigurationArtifactId,
            IEnumerable<int> batchIds)
        {
            _log.LogInformation(
                "Creating Progress Timer - " +
                "SourceWorkspaceId: {sourceWorkspaceId}, " +
                "DestinationWorkspaceId: {destinationWorkspaceId}, " +
                "JobHistoryId: {jobHistoryId}, " +
                "ImportJobId: {importJobId}",
                sourceWorkspaceId,
                destinationWorkspaceId,
                jobHistoryId,
                importJobId);

            _sourceWorkspaceId = sourceWorkspaceId;
            _importJobId = importJobId;
            _jobHistoryId = jobHistoryId;
            _syncConfigurationArtifactId = syncConfigurationArtifactId;
            _batchesArtifactIds = batchIds.ToList();
        }

        private async Task<Progress> GetBatchesProgressAsync()
        {
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
            int readRecordsCount = _readDocumentsCountCache + readDocumentsCount;
            int failedRecordsCount = _failedReadDocumentsCountCache + failedReadDocumentsCount;

            _readDocumentsCountCache = readDocumentsCountCache;
            _failedReadDocumentsCountCache = failedReadDocumentsCountCache;

            return new Progress(readRecordsCount, failedRecordsCount, 0);
        }

        private async Task<Progress> GetImportJobProgressAsync()
        {
            ImportProgress importProgress = await _importService.GetJobImportProgressValueAsync().ConfigureAwait(false);
            return new Progress(0, importProgress.ErroredRecords, importProgress.ImportedRecords);
        }
    }
}
