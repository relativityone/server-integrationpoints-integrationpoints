using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IJobHistoryErrorRepository
	{
		/// <summary>
		/// Mass edits the Job History Errors 
		/// </summary>
		/// <param name="claimsPrincipal">A ClaimsPrincipal object that contains the identity of the user</param>
		/// <param name="sourceWorkspaceId">Artifact ID of the source workspace</param>
		/// <param name="errorStatus">The error status to update the errors to</param>
		/// <param name="tableSuffix">Unique suffix that is appended to the scratch table</param>
		void UpdateErrorStatuses(ClaimsPrincipal claimsPrincipal, int sourceWorkspaceId, Relativity.Client.Choice errorStatus, string tableSuffix);
		
		/// <summary>
		/// Retrieves the Job History Errors for the given Job History Artifact Id
		/// </summary>
		/// <param name="jobHistoryArtifactId">Job History Artifact Id to gather job history errors for</param>
		List<JobHistoryError> RetreiveJobHistoryErrors(int jobHistoryArtifactId);
		
		/// <summary>
		/// Creates a saved search to temporarily be used for retry error jobs.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <param name="savedSearchArtifactId">The saved search artifact id used for the integration point job.</param>
		/// <param name="jobHistoryArtifactId">The job history artifact id to be retried.</param>
		/// <returns>The artifact id of the saved search to be deleted after job completion.</returns>
		int CreateItemLevelErrorsSavedSearch(int workspaceArtifactId, int savedSearchArtifactId, int jobHistoryArtifactId);
	}
}
