using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class BatchRepository : IBatchRepository
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly IDateTime _dateTime;

		public BatchRepository(ISourceServiceFactoryForAdmin serviceFactory, IDateTime dateTime)
		{
			_serviceFactory = serviceFactory;
			_dateTime = dateTime;
		}

		public Task<IBatch> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int totalItemsCount, int startingIndex)
		{
			return Batch.CreateAsync(_serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, totalItemsCount, startingIndex);
		}

		public Task<IBatch> GetAsync(int workspaceArtifactId, int artifactId)
		{
			return Batch.GetAsync(_serviceFactory, workspaceArtifactId, artifactId);
		}

		public Task<IEnumerable<IBatch>> GetAllAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			return Batch.GetAllAsync(_serviceFactory, workspaceArtifactId, syncConfigurationArtifactId);
		}

		public Task DeleteAllForConfigurationAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			return Batch.DeleteAllForConfigurationAsync(_serviceFactory, workspaceArtifactId, syncConfigurationArtifactId);
		}

		public Task DeleteAllOlderThanAsync(int workspaceArtifactId, TimeSpan olderThan)
		{
			return Batch.DeleteAllOlderThanAsync(_serviceFactory, _dateTime, workspaceArtifactId, olderThan);
		}

		public Task<IBatch> GetLastAsync(int workspaceArtifactId, int syncConfigurationId)
		{
			return Batch.GetLastAsync(_serviceFactory, workspaceArtifactId, syncConfigurationId);
		}

		public Task<IEnumerable<int>> GetAllNewBatchesIdsAsync(int workspaceArtifactId, int syncConfigurationId)
		{
			return Batch.GetAllNewBatchIdsAsync(_serviceFactory, workspaceArtifactId, syncConfigurationId);
		}

		public Task<IBatch> GetNextAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int startingIndex)
		{
			return Batch.GetNextAsync(_serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, startingIndex);
		}
	}
}