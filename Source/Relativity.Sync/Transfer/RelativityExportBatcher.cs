using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Extensions;
using Relativity.Sync.Kepler.Snapshot;
using Relativity.Sync.Kepler.SyncBatch;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using System.Diagnostics;

namespace Relativity.Sync.Transfer
{
    internal sealed class RelativityExportBatcher : IRelativityExportBatcher
    {
        private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly Guid _runId;
        private readonly int _sourceWorkspaceArtifactId;
        private readonly SyncBatchDto _syncBatchDto;
        private readonly IAPILog _logger;
        private readonly SemaphoreSlim _semaphoreSlim;

        private int _batchCurrentIndex;
        private int _remainingItems;
        private int _currentRetry;

        public RelativityExportBatcher(ISourceServiceFactoryForUser serviceFactoryForUser, IBatch batch, int sourceWorkspaceArtifactId)
        {
            _serviceFactoryForUser = serviceFactoryForUser;
            _runId = batch.ExportRunId;
            _sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;

            _batchCurrentIndex = batch.StartingIndex;
            _remainingItems = batch.TotalDocumentsCount - batch.TransferredDocumentsCount;

            _semaphoreSlim = new SemaphoreSlim(1, 1);
        }

        public RelativityExportBatcher(
           ISnapshotRepository snapshotRepository,
           SyncBatchDto batch,
           int sourceWorkspaceArtifactId,
           Func<SyncBatchDto, int> startingIndexFunc,
           Func<SyncBatchDto, int> remainingItemsFunc,
           IAPILog logger)
        {
            _snapshotRepository = snapshotRepository;
            _runId = batch.ExportRunId;
            _sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
            _syncBatchDto = batch;
            _logger = logger;

            _batchCurrentIndex = startingIndexFunc(batch);
            _remainingItems = remainingItemsFunc(batch);
        }

        public async Task<RelativityObjectSlim[]> GetNextItemsFromBatchAsync(CancellationToken cancellationToken)
        {
            // calling RetrieveResultsBlockFromExportAsync with remainingItems = 0 deletes the snapshot table
            if (_remainingItems == 0)
            {
                return Array.Empty<RelativityObjectSlim>();
            }

            using (IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                // Export API may not return all items in our batch (the actual results block size is configurable on the instance level),
                // so we have to account for results paging.
                RelativityObjectSlim[] block = await objectManager
                    .RetrieveResultsBlockFromExportAsync(_sourceWorkspaceArtifactId, _runId, _remainingItems, _batchCurrentIndex)
                    .ConfigureAwait(false);

                _batchCurrentIndex += block.Length;
                _remainingItems -= block.Length;

                return block;
            }
        }

        public void Dispose()
        {
            _semaphoreSlim?.Dispose();
        }

        private async Task<RelativityObjectSlim[]> GetNextItemsWithTimeoutAsync(int resultBlockSize, CancellationToken cancellationToken)
        {
            TimeSpan timeout = TimeSpan.FromMinutes(5);

            _logger.LogInformation("Retrieving {resultsBlockSize} from batch with timeout: {timeout}", resultBlockSize, timeout);

            try
            {
                await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
                RelativityObjectSlim[] result = await Task
               .Run(() => GetNextItemsFromBatchInternalAsync(resultBlockSize), cancellationToken)
                .TimeoutAfter(timeout)
                .ConfigureAwait(false);

                return result;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task<RelativityObjectSlim[]> GetNextItemsFromBatchInternalAsync(int resultsBlockSize)
        {
            // calling RetrieveResultsBlockFromExportAsync with remainingItems = 0 deletes the snapshot table
            if (_remainingItems == 0)
            {
                _logger.LogInformation("No remaining items left for batch {batchId}", _syncBatchDto.ArtifactId);
                return Array.Empty<RelativityObjectSlim>();
            }

            _logger.LogInformation("Retrieving {resultsBlockSize} results block from export using Object Manager API. Run ID: {runId} Index: {index} Remaining items: {remainingItems}", resultsBlockSize, _runId, _batchCurrentIndex, _remainingItems);

            int blockSize = _remainingItems < resultsBlockSize ? _remainingItems : resultsBlockSize;
            Stopwatch sw = Stopwatch.StartNew();

            RelativityObjectSlim[] block = await _snapshotRepository.ReadSnapshotResultsAsync(
                    _sourceWorkspaceArtifactId, _runId, blockSize, _batchCurrentIndex, Identity.CurrentUser)
                .ConfigureAwait(false);

            sw.Stop();

            int numberOfResults = block.Length;
            _logger.LogInformation("Retrieved {count} records from export, Time: {elapsedTime}, Used Retries: {retryCount}", numberOfResults, TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds), _currentRetry);

            _batchCurrentIndex += numberOfResults;
            _remainingItems -= numberOfResults;

            return block;
        }
    }
}
