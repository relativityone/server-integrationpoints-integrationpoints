using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
    internal interface IBatchRepository
    {
        /// <summary>
        /// Creates batch for given sync configuration.
        /// </summary>
        Task<IBatch> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, Guid exportRunId, int totalDocumentsCount, int startingIndex);

        /// <summary>
        /// Gets batch based on artifact ID.
        /// </summary>
        Task<IBatch> GetAsync(int workspaceArtifactId, int artifactId);

        /// <summary>
        /// Returns all batches belonging to the particular job.
        /// </summary>
        Task<IEnumerable<IBatch>> GetAllAsync(int workspaceArtifactId, int syncConfigurationArtifactId, Guid exportRunId);

        /// <summary>
        /// Deletes all batches belonging to the particular job.
        /// </summary>
        Task DeleteAllForConfigurationAsync(int workspaceArtifactId, int syncConfigurationArtifactId);

        /// <summary>
        /// Returns batch with highest starting index, which is always the last batch. Null if no batches found.
        /// </summary>
        Task<IBatch> GetLastAsync(int workspaceArtifactId, int syncConfigurationId, Guid exportRunId);

        /// <summary>
        /// Returns all batches that have not been finished - first Paused, then New.
        /// </summary>
        Task<IEnumerable<int>> GetAllBatchesIdsToExecuteAsync(int workspaceArtifactId, int syncConfigurationId, Guid exportRunId);

        /// <summary>
        /// Returns all batches that have successfully execute - first Completed, then Completed with Errors.
        /// </summary>
        Task<IEnumerable<IBatch>> GetAllSuccessfullyExecutedBatchesAsync(int workspaceArtifactId, int syncConfigurationId, Guid exportRunId);

        /// <summary>
        /// Returns batch with lowest starting index higher than given one. Null if no such batch found.
        /// </summary>
        Task<IBatch> GetNextAsync(int workspaceArtifactId, int syncConfigurationArtifactId, Guid exportRunId, int startingIndex);
    }
}