using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.ExecutionConstrains
{
    internal abstract class BaseSynchronizationExecutionConstrains : IExecutionConstrains<ISynchronizationConfiguration>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly ISyncLog _syncLog;

        public BaseSynchronizationExecutionConstrains(IBatchRepository batchRepository, ISyncLog syncLog)
        {
            _batchRepository = batchRepository;
            _syncLog = syncLog;
        }

        public async Task<bool> CanExecuteAsync(ISynchronizationConfiguration configuration, CancellationToken token)
        {
            bool canExecute = true;
            try
            {
                IEnumerable<int> batchIds = await _batchRepository.GetAllBatchesIdsToExecuteAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId, configuration.ExportRunId).ConfigureAwait(false);
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
