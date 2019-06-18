using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
	internal interface IBatchRepository
	{
		/// <summary>
		/// Creates batch for given sync configuration.
		/// </summary>
		Task<IBatch> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int totalItemsCount, int startingIndex);

		/// <summary>
		/// Gets batch based on artifact ID.
		/// </summary>
		Task<IBatch> GetAsync(int workspaceArtifactId, int artifactId);

		/// <summary>
		/// Returns all batches belonging to the particular job.
		/// </summary>
		Task<IEnumerable<IBatch>> GetAllAsync(int workspaceArtifactId, int syncConfigurationArtifactId);

		/// <summary>
		/// Returns batch with highest starting index, which is always the last batch. Null if no batches found.
		/// </summary>
		Task<IBatch> GetLastAsync(int workspaceArtifactId, int syncConfigurationId);

		/// <summary>
		/// Returns all batches that has not been started yet (have status New).
		/// </summary>
		Task<IEnumerable<int>> GetAllNewBatchesIdsAsync(int workspaceArtifactId, int syncConfigurationId);

		/// <summary>
		/// Returns batch with lowest starting index higher than given one. Null if no such batch found.
		/// </summary>
		Task<IBatch> GetNextAsync(int workspaceArtifactId, int syncConfigurationArtifactId, int startingIndex);
	}
}