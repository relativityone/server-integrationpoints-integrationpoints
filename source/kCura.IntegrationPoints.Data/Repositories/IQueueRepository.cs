namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IQueueRepository
	{
		/// <summary>
		/// Queries the ScheduleAgentQueue table to determine if there are any jobs that belong to the same workspace and Integration Point currently in the queue or running. 
		/// NOTE: Scheduled jobs are intentionally excluded from the result..
		/// </summary>
		/// <param name="workspaceId">The Artifact ID of the workspace that the job is run from.</param>
		/// <param name="integrationPointId">The Artifact ID of the Integration Point the job belongs to.</param>
		/// <returns>True if there are jobs running or in the queue, false otherwise.</returns>
		bool HasJobsExecutingOrInQueue(int workspaceId, int integrationPointId);

		/// <summary>
		/// Queries the ScheduleAgentQueue table to determine if there are any jobs that belong to the same workspace and Integration Point currently running.
		/// </summary>
		/// <param name="workspaceId">The Artifact ID of the workspace that the job is run from.</param>
		/// <param name="integrationPointId">The Artifact ID of the Integration Point the job belongs to.</param>
		/// <returns>True if there are jobs running, false otherwise.</returns>
		bool HasJobsExecuting(int workspaceId, int integrationPointId);
	}
}
