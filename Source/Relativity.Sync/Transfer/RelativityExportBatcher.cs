using System;
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
		private int _lastStartingIndex;

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

			_lastStartingIndex = -1;
		}

		public async Task<RelativityObjectSlim[]> GetNextBatchAsync()
		{
			IBatch nextBatch = await _batchRepository.GetNextAsync(_sourceWorkspaceArtifactId, _syncConfigurationArtifactId, _lastStartingIndex).ConfigureAwait(false);

			if (nextBatch == null)
			{
				// Since we don't update _lastStartingIndex if nextBatch is null, every invocation of this method will return empty after
				// the first one returns empty: we'll use the same index each time we call GetNextAsync, which will return null each time.

				return Array.Empty<RelativityObjectSlim>();
			}

			_lastStartingIndex = nextBatch.StartingIndex;

			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				int resultsBlockSize = nextBatch.TotalItemsCount;
				int startingIndex = nextBatch.StartingIndex;

				RelativityObjectSlim[] block = await objectManager
					.RetrieveResultsBlockFromExportAsync(_sourceWorkspaceArtifactId, _runId, resultsBlockSize, startingIndex)
					.ConfigureAwait(false);
				return block;
			}
		}
	}
}
