using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.ExecutionConstrains
{
	internal sealed class SynchronizationExecutorConstrains : IExecutionConstrains<ISynchronizationConfiguration>
	{
		private readonly IBatchRepository _batchRepository;
		private readonly ISyncLog _syncLog;

		public SynchronizationExecutorConstrains(IBatchRepository batchRepository, ISyncLog syncLog)
		{
			_batchRepository = batchRepository;
			_syncLog = syncLog;
		}

		public async Task<bool> CanExecuteAsync(ISynchronizationConfiguration configuration, CancellationToken token)
		{
			bool canExecute = true;
			try
			{
				IEnumerable<int> batchIds = await _batchRepository.GetAllNewBatchesIdsAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false);
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