using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Extensions;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	internal sealed class RelativityExportBatcher : IRelativityExportBatcher
	{
		private int _batchCurrentIndex;
		private int _remainingItems;

		private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
		private readonly Guid _runId;
		private readonly int _sourceWorkspaceArtifactId;

		public RelativityExportBatcher(ISourceServiceFactoryForUser serviceFactoryForUser, IBatch batch, int sourceWorkspaceArtifactId)
		{
			_serviceFactoryForUser = serviceFactoryForUser;
			_runId = batch.ExportRunId;
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;

			_batchCurrentIndex = batch.StartingIndex;
			_remainingItems = batch.TotalDocumentsCount - batch.TransferredDocumentsCount;
		}

		public async Task<RelativityObjectSlim[]> GetNextItemsFromBatchAsync(CancellationToken cancellationToken)
		{
			TimeSpan timeout = TimeSpan.FromMinutes(15);
			// calling RetrieveResultsBlockFromExportAsync with remainingItems = 0 deletes the snapshot table
			if (_remainingItems == 0)
			{
				Relativity.Logging.Log.Logger.LogInformation("No remaining items left for batch {batchId}", _runId);
				return Array.Empty<RelativityObjectSlim>();
			}

			Relativity.Logging.Log.Logger.LogInformation("Retrieving results block from export using Object Manager API. Run ID: {runId} Index: {index} Remaining items: {remainingItems}", _runId, _batchCurrentIndex, _remainingItems);

			RelativityObjectSlim[] block = await Task.Run(
					() => ReadSnapshotResultsAsync(_sourceWorkspaceArtifactId, _runId, _remainingItems,
						_batchCurrentIndex), cancellationToken)
				.TimeoutAfter(timeout).ConfigureAwait(false);

			_batchCurrentIndex += block.Length;
			_remainingItems -= block.Length;

			Relativity.Logging.Log.Logger.LogInformation("Retrieved {count} records from export. Index: {index} Remaining items: {remainingItems}", _remainingItems, _batchCurrentIndex, _remainingItems);
			return block;
			
		}
		private async Task<RelativityObjectSlim[]> ReadSnapshotResultsAsync(int workspaceId, Guid snapshotId, int resultsBlockSize, int exportIndex)
		{
			using IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);
			return await objectManager
				.RetrieveResultsBlockFromExportAsync(workspaceId, snapshotId, resultsBlockSize, exportIndex)
				.ConfigureAwait(false);
		}
	}
}
