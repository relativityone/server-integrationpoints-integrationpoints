﻿using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class BatchRepository : IBatchRepository
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;

		public BatchRepository(ISourceServiceFactoryForAdmin serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task<IBatch> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int totalItemsCount, int startingIndex)
		{
			return await Batch.CreateAsync(_serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, totalItemsCount, startingIndex).ConfigureAwait(false);
		}

		public async Task<IBatch> GetAsync(int workspaceArtifactId, int artifactId)
		{
			return await Batch.GetAsync(_serviceFactory, workspaceArtifactId, artifactId).ConfigureAwait(false);
		}

		public async Task<IBatch> GetLastAsync(int workspaceArtifactId, int syncConfigurationId)
		{
			return await Batch.GetLastAsync(_serviceFactory, workspaceArtifactId, syncConfigurationId).ConfigureAwait(false);
		}

		public async Task<IBatch> GetFirstAsync(int workspaceArtifactId, int syncConfigurationId)
		{
			return await Batch.GetFirstAsync(_serviceFactory, workspaceArtifactId, syncConfigurationId).ConfigureAwait(false);
		}

		public async Task<IBatch> GetNextAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int startingIndex)
		{
			return await Batch.GetNextAsync(_serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, startingIndex).ConfigureAwait(false);
		}
	}
}