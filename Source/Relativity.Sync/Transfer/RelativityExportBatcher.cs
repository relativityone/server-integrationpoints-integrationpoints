using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	internal delegate IRelativityExportBatcher RelativityExportBatcherFactory(Guid runId, int sourceWorkspaceArtifactId, int syncConfigurationArtifactId);

	internal sealed class RelativityExportBatcher : IRelativityExportBatcher
	{
		private int _previousStartingIndex;

		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly IBatchRepository _batchRepository;
		private readonly Guid _runId;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly int _syncConfigurationArtifactId;

		public RelativityExportBatcher(ISourceServiceFactoryForUser serviceFactory, IBatchRepository batchRepository, Guid runId, int sourceWorkspaceArtifactId, int syncConfigurationArtifactId)
		{
			_serviceFactory = serviceFactory;
			_batchRepository = batchRepository;
			_runId = runId;
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_syncConfigurationArtifactId = syncConfigurationArtifactId;

			_previousStartingIndex = -1;
		}

		public async Task<RelativityObjectSlim[]> GetNextBatchAsync()
		{
			IBatch nextBatch = await _batchRepository.GetNextAsync(_sourceWorkspaceArtifactId, _syncConfigurationArtifactId, _previousStartingIndex).ConfigureAwait(false);

			if (nextBatch == null)
			{
				// Since we don't update _previousStartingIndex if nextBatch is null, every invocation of this method will return empty after
				// the first one returns empty: we'll use the same index each time we call GetNextAsync, which will return null each time.

				return Array.Empty<RelativityObjectSlim>();
			}

			_previousStartingIndex = nextBatch.StartingIndex;

			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				List<RelativityObjectSlim> batchItems = new List<RelativityObjectSlim>();

				int batchStartingIndex = nextBatch.StartingIndex;
				int batchEndingIndex = batchStartingIndex + nextBatch.TotalItemsCount;

				// Export API may not return all items in our batch (the actual results block size is configurable on the instance level),
				// so we have to account for results paging.

				int currentExportIndex = batchStartingIndex;
				while (currentExportIndex < batchEndingIndex)
				{
					int remainingItemsInBatch = batchEndingIndex - currentExportIndex;
					RelativityObjectSlim[] block = await objectManager
						.RetrieveResultsBlockFromExportAsync(_sourceWorkspaceArtifactId, _runId, remainingItemsInBatch, currentExportIndex)
						.ConfigureAwait(false);

					currentExportIndex += block.Length;
					batchItems.AddRange(block);
				}

				return batchItems.ToArray();
			}
		}
	}
}
