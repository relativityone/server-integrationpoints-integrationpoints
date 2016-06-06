﻿using System.Collections.Generic;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IJobHistoryErrorRepository
	{
		/// <summary>
		/// Retrieves the Job History Errors for the given Job History Artifact Id
		/// </summary>
		/// <param name="jobHistoryArtifactId">Job History Artifact Id to gather job history errors for</param>
		/// <param name="errorType">Error Type choice to gather job history errors for</param>
		/// <returns>List of Artifact Ids of Job History Errors for the provided Job History and Error Type</returns>
		IList<int> RetrieveJobHistoryErrorArtifactIds(int jobHistoryArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values errorType);

		/// <summary>
		/// Mass edits the Job History Errors 
		/// </summary>
		/// <param name="claimsPrincipal">A ClaimsPrincipal object that contains the identity of the user</param>
		/// <param name="numberOfErrors">The number of errors to be updated</param>
		/// <param name="jobHistoryErrorTypeId">Type Id of the Job History Error</param>
		/// <param name="errorStatusArtifactId">The Error Status Artifact Id to update the errors to</param>
		/// <param name="tableName">Unique temp table name to run updates with</param>
		void UpdateErrorStatuses(ClaimsPrincipal claimsPrincipal, int numberOfErrors, int jobHistoryErrorTypeId, int errorStatusArtifactId, string tableName);

		/// <summary>
		/// Creates a saved search to temporarily be used for retry error jobs.
		/// </summary>
		/// <param name="integrationPointArtifactId">The integration point artifact id.</param>
		/// <param name="savedSearchArtifactId">The saved search artifact id used for the integration point job.</param>
		/// <param name="jobHistoryArtifactId">The job history artifact id to be retried.</param>
		/// <returns>The artifact id of the saved search to be deleted after job completion.</returns>
		int CreateItemLevelErrorsSavedSearch(int integrationPointArtifactId, int savedSearchArtifactId, int jobHistoryArtifactId);

		/// <summary>
		/// Deletes the saved search used for the item-level retry error job.
		/// </summary>
		/// <param name="savedSearchArtifactId">The artifact id of the temporary saved search made.</param>
		/// <param name="retryAttempts">The amount of times this method has been called as part of a retry recursive loop.</param>
		void DeleteItemLevelErrorsSavedSearch(int savedSearchArtifactId, int retryAttempts);

		/// <summary>
		/// Reads specified job history error instances.
		/// </summary>
		/// <param name="artifactIds">Artifact ids of job history errors to read.</param>
		/// <returns>Object representations of job history errors.</returns>
		IList<JobHistoryErrorDTO> Read(IEnumerable<int> artifactIds);

		/// <summary>
		/// Retrieves the Job History Error artifact ids and Source Unique ids
		/// </summary>
		/// <param name="jobHistoryArtifactId">Job History artifact id</param>
		/// <param name="errorType">Error type choice</param>
		/// <returns>Dictionary of Job History Error artifact ids and corresponding Source Unique ids</returns>
		IDictionary<int, string> RetrieveJobHistoryErrorIdsAndSourceUniqueIds(int jobHistoryArtifactId, JobHistoryErrorDTO.Choices.ErrorType.Values errorType);
	}
}
