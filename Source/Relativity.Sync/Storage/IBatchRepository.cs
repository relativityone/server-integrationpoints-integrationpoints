using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
	internal interface IBatchRepository
	{
		Task<IBatch> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int totalItemsCount, int startingIndex);
		Task<IBatch> GetAsync(int workspaceArtifactId, int artifactId);
	}
}