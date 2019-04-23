﻿using System;
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

		public async Task<bool> CanExecuteAsync(ISnapshotPartitionConfiguration configuration, CancellationToken token)
		{
			try
			{
				IBatch batch = await _batchRepository.GetLastAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false);

				if (batch == null)
				{
					// no batches created yet
					return true;
				}

				int numberOfRecordsIncludedInBatches = batch.StartingIndex + batch.TotalItemsCount;

				// we should execute if number of records included in batches in lower than total number of records
				return numberOfRecordsIncludedInBatches < configuration.TotalRecordsCount;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occurred when looking for created batches.");
				throw;
			}
		}
	}
}