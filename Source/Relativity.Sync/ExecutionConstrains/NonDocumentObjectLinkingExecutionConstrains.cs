using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class NonDocumentObjectLinkingExecutionConstrains : IExecutionConstrains<INonDocumentObjectLinkingConfiguration>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IAPILog _syncLog;

        public NonDocumentObjectLinkingExecutionConstrains(IBatchRepository batchRepository, IAPILog syncLog)
        {
            _batchRepository = batchRepository;
            _syncLog = syncLog;
        }

        public async Task<bool> CanExecuteAsync(INonDocumentObjectLinkingConfiguration configuration,
            CancellationToken token)
        {
            bool canExecute = true;
            try
            {
                if (!configuration.ObjectLinkingSnapshotId.HasValue)
                {
                    _syncLog.LogInformation(
                        $"{nameof(INonDocumentObjectLinkingConfiguration.ObjectLinkingSnapshotId)} is empty - skipping object linking");

                    return false;
                }

                IEnumerable<int> batchIds = await _batchRepository
                    .GetAllBatchesIdsToExecuteAsync(configuration.SourceWorkspaceArtifactId,
                        configuration.SyncConfigurationArtifactId, configuration.ObjectLinkingSnapshotId.Value)
                    .ConfigureAwait(false);
                if (batchIds == null || !batchIds.Any())
                {
                    canExecute = false;
                }
            }
            catch (Exception exception)
            {
                _syncLog.LogError(exception, "Exception occurred when reviewing batches and batch status.");
                throw;
            }

            return canExecute;
        }
    }
}
