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
		private IBatch _currentBatch;

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

			_currentBatch = null;
		}

		public async Task<RelativityObjectSlim[]> GetNextBatchAsync()
		{
			IBatch nextBatch = _currentBatch == null
				? await _batchRepository.GetFirstAsync(_sourceWorkspaceArtifactId, _syncConfigurationArtifactId).ConfigureAwait(false)
				: await _batchRepository.GetNextAsync(_sourceWorkspaceArtifactId, _syncConfigurationArtifactId, _currentBatch.StartingIndex).ConfigureAwait(false);

			if (nextBatch == null)
			{
				return Array.Empty<RelativityObjectSlim>();
			}

			_currentBatch = nextBatch;

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
