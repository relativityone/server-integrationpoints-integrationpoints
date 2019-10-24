using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.System.Stubs
{
	internal sealed class BatchRepositoryStub : IBatchRepository
	{
		private readonly List<IBatch> _batches;

		public BatchRepositoryStub(List<IBatch> batches)
		{
			_batches = batches.OrderBy(b => b.StartingIndex).ToList();
		}

		public Task<IBatch> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int totalItemsCount, int startingIndex)
		{
			throw new NotImplementedException();
		}

		public Task<IBatch> GetAsync(int workspaceArtifactId, int artifactId)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<IBatch>> GetAllAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAllForConfigurationAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAllOlderThanAsync(int workspaceArtifactId, TimeSpan olderThan)
		{
			throw new NotImplementedException();
		}

		public Task<IBatch> GetLastAsync(int workspaceArtifactId, int syncConfigurationId)
		{
			return Task.FromResult(_batches.Last());
		}

		public Task<IEnumerable<int>> GetAllNewBatchesIdsAsync(int workspaceArtifactId, int syncConfigurationId)
		{
			throw new NotImplementedException();
		}

		public Task<IBatch> GetNextAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int startingIndex)
		{
			IBatch nextBatch = _batches.FirstOrDefault(b => b.StartingIndex > startingIndex);
			return Task.FromResult(nextBatch);
		}

		public static IBatchRepository Create(int totalItemCount, int batchSize, int startingIndex = 0)
		{
			List<IBatch> batches = new List<IBatch>();
			for (int i = startingIndex; i < totalItemCount; i += batchSize)
			{
				int totalItemsInBatch = Math.Min(batchSize, totalItemCount - i);
				IBatch newBatch = new BatchStub(i, totalItemsInBatch, i);
				batches.Add(newBatch);
			}

			return new BatchRepositoryStub(batches);
		}
	}
}
