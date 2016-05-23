﻿using System.Collections.Generic;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using Relativity.Toggles;

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
		List<int> RetrieveJobHistoryErrorArtifactIds(int jobHistoryArtifactId, Relativity.Client.Choice errorType);


		/// <summary>
		/// Mass edits the Job History Errors 
		/// </summary>
		/// <param name="claimsPrincipal">A ClaimsPrincipal object that contains the identity of the user</param>
		/// <param name="numberOfErrors">The number of errors to be updated</param>
		/// <param name="jobHistoryErrorTypeId">Type Id of the Job History Error</param>
		/// <param name="sourceWorkspaceId">Artifact Id of the source workspace</param>
		/// <param name="errorStatusArtifactId">The Error Status Artifact Id to update the errors to</param>
		/// <param name="tableName">Unique temp table name to run updates with</param>
		void UpdateErrorStatuses(ClaimsPrincipal claimsPrincipal, int numberOfErrors, int jobHistoryErrorTypeId, int sourceWorkspaceId, int errorStatusArtifactId, string tableName);

		/// <summary>
		/// Creates a saved search to temporarily be used for retry error jobs.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <param name="integrationPointArtifactId">The integration point artifact id.</param>
		/// <param name="savedSearchArtifactId">The saved search artifact id used for the integration point job.</param>
		/// <param name="jobHistoryArtifactId">The job history artifact id to be retried.</param>
		/// <param name="userArtifactId.">The artifact id of the user executing the retry job.</param>
		/// <returns>The artifact id of the saved search to be deleted after job completion.</returns>
		int CreateItemLevelErrorsSavedSearch(int workspaceArtifactId, int integrationPointArtifactId, int savedSearchArtifactId, int jobHistoryArtifactId, int userArtifactId);

		/// <summary>
		/// Deletes the saved search used for the item-level retry error job.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <param name="savedSearchArtifactId">The artifact id of the temporary saved search made.</param>
		/// <param name="retryAttempts">The amount of times this method has been called as part of a retry recursive loop.</param>
		void DeleteItemLevelErrorsSavedSearch(int workspaceArtifactId, int savedSearchArtifactId, int retryAttempts);
	}
}
