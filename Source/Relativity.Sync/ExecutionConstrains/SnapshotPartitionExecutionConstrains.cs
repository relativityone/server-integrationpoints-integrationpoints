using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.ExecutionConstrains
{
	internal sealed class SnapshotPartitionExecutionConstrains : IExecutionConstrains<ISnapshotPartitionConfiguration>
	{
		private readonly IBatchRepository _batchRepository;
		private readonly ISyncLog _logger;

		public SnapshotPartitionExecutionConstrains(IBatchRepository batchRepository, ISyncLog logger)
		{
			_batchRepository = batchRepository;
			_logger = logger;
		}

		public Task<bool> CanExecuteAsync(ISnapshotPartitionConfiguration configuration, CancellationToken token)
		{
			try
			{
				return Task.FromResult(true);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occurred when looking for created batches.");
				throw;
			}
		}
	}
}