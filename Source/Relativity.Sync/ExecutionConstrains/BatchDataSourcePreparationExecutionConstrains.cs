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
    internal class BatchDataSourcePreparationExecutionConstrains : IExecutionConstrains<IBatchDataSourcePreparationConfiguration>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IAPILog _logger;

        public BatchDataSourcePreparationExecutionConstrains(IBatchRepository batchRepository, IAPILog syncLog)
        {
            _batchRepository = batchRepository;
            _logger = syncLog;
        }

        public async Task<bool> CanExecuteAsync(IBatchDataSourcePreparationConfiguration configuration, CancellationToken token)
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
                _logger.LogError(exception, "Exception occurred when reviewing batches and batch status.");
                throw;
            }

            return canExecute;
        }
    }
}
