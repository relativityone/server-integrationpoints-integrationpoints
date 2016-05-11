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
	}
}
