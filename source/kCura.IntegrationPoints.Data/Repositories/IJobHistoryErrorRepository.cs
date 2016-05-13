using System.Collections.Generic;
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
		List<int> RetreiveJobHistoryErrorArtifactIds(int jobHistoryArtifactId, Relativity.Client.Choice errorType);
		
		/// <summary>
		/// Determines the Update Status Type that will be used to know which temp tables to create and which to use for started and completed status updates
		/// </summary>
		/// <param name="jobType">Job Type for the new job being run</param>
		/// <param name="hasJobLevelErrors">Boolean of if Job-level errors exist associated with the last Job History object</param>
		/// <param name="hasItemLevelErrors">Boolean of if Item-level errors exist associated with the last Job History object</param>
		/// <returns>An UpdateStatusType that houses the job type and error types to make error status changes with</returns>
		JobHistoryErrorDTO.UpdateStatusType DetermineUpdateStatusType(Relativity.Client.Choice jobType, bool hasJobLevelErrors, bool hasItemLevelErrors);

		/// <summary>
		/// Creates the unique temp tables required to make Error Status updates for currently running job
		/// </summary>
		/// <param name="jobLevelErrors">List of Artifact Ids of Job-level Job History Errors for the previous Job History</param>
		/// <param name="itemLevelErrors">List of Artifact Ids of Item-level Job History Errors for the previous Job History</param>
		/// <param name="updateStatusType">UpdateStatusType that houses the job type and error types to know which temp tables to create</param>
		/// <param name="uniqueJobId">Job Id and Job Guid combined to be a suffix for the temp table</param>
		void CreateErrorListTempTables(List<int> jobLevelErrors, List<int> itemLevelErrors, JobHistoryErrorDTO.UpdateStatusType updateStatusType, string uniqueJobId);

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
		/// <param name="savedSearchArtifactId">The saved search artifact id used for the integration point job.</param>
		/// <param name="jobHistoryArtifactId">The job history artifact id to be retried.</param>
		/// <returns>The artifact id of the saved search to be deleted after job completion.</returns>
		int CreateItemLevelErrorsSavedSearch(int workspaceArtifactId, int savedSearchArtifactId, int jobHistoryArtifactId);
	}
}
