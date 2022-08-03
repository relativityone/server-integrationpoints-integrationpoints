using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IJobHistoryErrorRepository : IRepositoryWithMassUpdate
    {
        /// <summary>
        /// Retrieves the Job History Errors for the given Job History Artifact Id
        /// </summary>
        /// <param name="jobHistoryArtifactId">Job History Artifact Id to gather job history errors for</param>
        /// <param name="errorType">Error Type choice to gather job history errors for</param>
        /// <returns>Collection of Artifact Ids of Job History Errors for the provided Job History and Error Type</returns>
        ICollection<int> RetrieveJobHistoryErrorArtifactIds(int jobHistoryArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values errorType);

        /// <summary>
        /// Creates a saved search to temporarily be used for retry error jobs.
        /// </summary>
        /// <param name="integrationPointArtifactId">The integration point artifact id.</param>
        /// <param name="savedSearchArtifactId">The saved search artifact id used for the integration point job.</param>
        /// <param name="jobHistoryArtifactId">The job history artifact id to be retried.</param>
        /// <returns>The artifact id of the saved search to be deleted after job completion.</returns>
        int CreateItemLevelErrorsSavedSearch(int integrationPointArtifactId, int savedSearchArtifactId, int jobHistoryArtifactId);

        /// <summary>
        /// Attempt to deletes the saved search used for the item-level retry error job.
        /// </summary>
        /// <param name="savedSearchArtifactId">The artifact id of the temporary saved search made.</param>
        void DeleteItemLevelErrorsSavedSearch(int savedSearchArtifactId);

        /// <summary>
        /// Reads specified job history error instances.
        /// </summary>
        /// <param name="artifactIds">Artifact ids of job history errors to read.</param>
        /// <returns>Object representations of job history errors.</returns>
        IList<JobHistoryError> Read(IEnumerable<int> artifactIds);

        /// <summary>
        /// Retrieves the Job History Error artifact ids and Source Unique ids
        /// </summary>
        /// <param name="jobHistoryArtifactId">Job History artifact id</param>
        /// <param name="errorType">Error type choice</param>
        /// <returns>Dictionary of Job History Error artifact ids and corresponding Source Unique ids</returns>
        IDictionary<int, string> RetrieveJobHistoryErrorIdsAndSourceUniqueIds(int jobHistoryArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values errorType);
    }
}
