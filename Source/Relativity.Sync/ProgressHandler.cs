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

namespace Relativity.Sync
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

        private int _completedRecordsCountForGeneratedItems = 0;
        private int _failedRecordsCountForGeneratedItems = 0;
        private List<int> _batchesArtifactIds = new List<int>();

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

                _batchesArtifactIds = (await _batchRepository
                    .GetAllAsync(_sourceWorkspaceId, _syncConfigurationArtifactId, _importJobId)
                    .ConfigureAwait(false))
                    .Select(x => x.ArtifactId)
                    .ToList();

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
                if (!_batchesArtifactIds.Any())
                {
                    return;
                }

                ImportProgress importProgress = await GetImportJobProgressAsync().ConfigureAwait(false);

                List<IBatch> batches = (await _batchRepository.GetBatchesWithIdsAsync(_sourceWorkspaceId, _syncConfigurationArtifactId, _batchesArtifactIds, _importJobId)
                        .ConfigureAwait(false))
                    .Where(x => !_batchesArtifactIds.Contains(x.ArtifactId))
                    .ToList();

                int readDocumentsCount = 0;
                int failedReadDocumentsCount = 0;
                foreach (IBatch batch in batches)
                {
                    if (batch.Status == BatchStatus.Generated)
                    {
                        _batchesArtifactIds.Remove(batch.ArtifactId);
                    }
                    readDocumentsCount += batch.ReadDocumentsCount;
                    failedReadDocumentsCount += batch.FailedReadDocumentsCount;
                }

                int completedRecordsCount = _completedRecordsCountForGeneratedItems + readDocumentsCount + importProgress.ImportedRecords;
                int failedRecordsCount = _failedRecordsCountForGeneratedItems + failedReadDocumentsCount + importProgress.ErroredRecords;

                await _progressUpdater.UpdateJobProgressAsync(
                        _sourceWorkspaceId,
                        _jobHistoryId,
                        completedRecordsCount,
                        failedRecordsCount)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Sync Job progress update failed. Progress won't be updated in this cycle.");
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
