using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.ExecutionConstrains
{
    internal abstract class BaseSynchronizationExecutionConstrains<T> : IExecutionConstrains<T> where T : ISynchronizationConfiguration
    {
        protected readonly IBatchRepository _batchRepository;
        protected readonly ISyncLog SyncLog;

        public BaseSynchronizationExecutionConstrains(IBatchRepository batchRepository, ISyncLog syncLog)
        {
            _batchRepository = batchRepository;
            SyncLog = syncLog;
        }

        public virtual async Task<bool> CanExecuteAsync(T configuration, CancellationToken token)
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
                SyncLog.LogError(exception, "Exception occurred when reviewing batches and batch status.");
                throw;
            }
            return canExecute;
        }
	}
}
