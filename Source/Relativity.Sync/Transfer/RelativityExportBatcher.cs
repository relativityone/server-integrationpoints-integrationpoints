using System;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	internal sealed class RelativityExportBatcher : IRelativityExportBatcher
	{
		private int _batchCurrentIndex;
		private int _remainingItems;

		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly Guid _runId;
		private readonly int _sourceWorkspaceArtifactId;

		public RelativityExportBatcher(ISourceServiceFactoryForUser serviceFactory, IBatch batch, Guid runId, int sourceWorkspaceArtifactId)
		{
			_serviceFactory = serviceFactory;
			_runId = runId;
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;

			_batchCurrentIndex = batch.StartingIndex;
			_remainingItems = batch.TotalDocumentsCount - batch.TransferredDocumentsCount;
		}

		public async Task<RelativityObjectSlim[]> GetNextItemsFromBatchAsync()
		{
			// calling RetrieveResultsBlockFromExportAsync with remainingItems = 0 deletes the snapshot table 
			if (_remainingItems == 0)
			{
				return Array.Empty<RelativityObjectSlim>();
			}

			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
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
	}
}
