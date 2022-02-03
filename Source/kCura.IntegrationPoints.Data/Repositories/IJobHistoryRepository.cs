using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IJobHistoryRepository
	{
		/// <summary>
		/// Gets last finished Job History artifact id for a given Integration Point
		/// </summary>
		/// <param name="integrationPointArtifactId">Integration Point artifact id</param>
		/// <returns>Artifact id of the most recent finished Job History objects</returns>
		int GetLastJobHistoryArtifactId(int integrationPointArtifactId);
		/// <summary>
		/// We mark job as failed or validation failed. We change status.
		/// </summary>
		/// <param name="jobHistoryID">Integration Point job history id</param>
		/// <param name="integrationPointID">Integration Point id</param>
		void MarkJobAsValidationFailed(int jobHistoryID, int integrationPointID, DateTime jobEndTime);

		void MarkJobAsFailed(int jobHistoryID, int integrationPointID, DateTime jobEndTime);

		/// <summary>
		/// Gets the stoppable Job History artifact ids for a given Integration Point.
		/// </summary>
		/// <param name="integrationPointArtifactId">The parent Integration Point artifact id.</param>
		/// <returns>A dictionary where the JobHistoryStatus choice Guid is the key, and the value is an array of associated artifact ids.</returns>
		/// <remarks>The only two statuses that should return are Pending and Processing.</remarks>
		IDictionary<Guid, int[]> GetStoppableJobHistoryArtifactIdsByStatus(int integrationPointArtifactId);

		IList<JobHistory> GetStoppableJobHistoriesForIntegrationPoint(int integrationPointArtifactId);

		string GetJobHistoryName(int jobHistoryArtifactId);
	}
}
