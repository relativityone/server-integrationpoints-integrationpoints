using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
	internal interface IBatchRepository
	{
		/// <summary>
		///     Creates batch for given sync configuration
		/// </summary>
		Task<IBatch> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int totalItemsCount, int startingIndex);

		/// <summary>
		///     Gets batch based on artifact ID
		/// </summary>
		Task<IBatch> GetAsync(int workspaceArtifactId, int artifactId);

		/// <summary>
		///     Returns batch with highest starting index. Null if no batches found
		/// </summary>
		Task<IBatch> GetLastAsync(int workspaceArtifactId, int syncConfigurationId);

		/// <summary>
		///     Returns batch with lowest starting index. Null if no batches found
		/// </summary>
		Task<IBatch> GetFirstAsync(int workspaceArtifactId, int syncConfigurationId);

		/// <summary>
		///     Returns batch with lowest starting index higher than given one. Null if no such batch found
		/// </summary>
		Task<IBatch> GetNextAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int startingIndex);
	}
}