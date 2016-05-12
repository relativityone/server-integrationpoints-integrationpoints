using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IJobHistoryErrorManager
	{
		/// <summary>
		/// Gets Job History Errors for the last Job History object for a given Integration Point
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace artifact id</param>
		/// <param name="integrationPointArtifactId">Integration Point artifact id</param>
		/// <returns>List of Job History Errors</returns>
		List<JobHistoryError> GetLastJobHistoryErrors(int workspaceArtifactId, int integrationPointArtifactId);
		
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
