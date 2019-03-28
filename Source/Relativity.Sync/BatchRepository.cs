using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync
{
	internal sealed class BatchRepository
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;

		public BatchRepository(ISourceServiceFactoryForAdmin serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task<IBatch> CreateAsync(int syncConfigurationArtifactId, int totalItemsCount, int startingIndex)
		{
			Batch batch = new Batch(_serviceFactory, 1, syncConfigurationArtifactId, totalItemsCount, startingIndex);
			await batch.CreateAsync().ConfigureAwait(false);
			return batch;
		}

		public async Task<IBatch> GetAsync(int artifactId)
		{
			Batch batch = new Batch(_serviceFactory, 1, artifactId);
			await batch.ReadAsync().ConfigureAwait(false);
			return batch;
		}
	}
}