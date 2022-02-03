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

		IList<JobHistory> GetStoppableJobHistoriesForIntegrationPoint(int integrationPointArtifactId);

		string GetJobHistoryName(int jobHistoryArtifactId);
	}
}
