using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class ProgressRepository : IProgressRepository
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;

		public ProgressRepository(ISourceServiceFactoryForAdmin serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task<IProgress> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, string name, int order, string status)
		{
			return await Progress.CreateAsync(_serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, name, order, status).ConfigureAwait(false);
		}

		public async Task<IProgress> GetAsync(int workspaceArtifactId, int artifactId)
		{
			return await Progress.GetAsync(_serviceFactory, workspaceArtifactId, artifactId).ConfigureAwait(false);
		}
	}
}