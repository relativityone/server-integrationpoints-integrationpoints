using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
	internal interface IBatchRepository
	{
		Task<IBatch> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int totalItemsCount, int startingIndex);
		Task<IBatch> GetAsync(int workspaceArtifactId, int artifactId);

		/// <summary>
		///     Returns batch with highest starting index. Null if no batches found
		/// </summary>
		Task<IBatch> GetLastAsync(int workspaceArtifactId, int syncConfigurationId);
	}
}