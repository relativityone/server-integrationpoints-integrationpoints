﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class SnapshotPartitionExecutor : IExecutor<ISnapshotPartitionConfiguration>
	{
		private readonly IBatchRepository _batchRepository;
		private readonly ISyncLog _logger;

		public SnapshotPartitionExecutor(IBatchRepository batchRepository, ISyncLog logger)
		{
			_batchRepository = batchRepository;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ISnapshotPartitionConfiguration configuration, CancellationToken token)
		{
			IBatch batch;
			try
			{
				batch = await _batchRepository.GetLastAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unable to retrieve last batch for sync configuration {artifactId}.", configuration.SyncConfigurationArtifactId);
				return ExecutionResult.Failure("Cannot read last batch.", e);
			}

			int numberOfRecordsIncludedInBatches = 0;
			if (batch != null)
			{
				numberOfRecordsIncludedInBatches = batch.StartingIndex + batch.TotalItemsCount;
			}

			Snapshot snapshot = new Snapshot(configuration.TotalRecordsCount, configuration.BatchSize, numberOfRecordsIncludedInBatches);

			try
			{
				foreach (SnapshotPart snapshotPart in snapshot.GetSnapshotParts())
				{
					await _batchRepository.CreateAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId, snapshotPart.NumberOfRecords, snapshotPart.StartingIndex)
						.ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unable to create batch for sync configuration {artifactId}.", configuration.SyncConfigurationArtifactId);
				return ExecutionResult.Failure("Unable to create batches.", e);
			}

			return ExecutionResult.Success();
		}
	}
}