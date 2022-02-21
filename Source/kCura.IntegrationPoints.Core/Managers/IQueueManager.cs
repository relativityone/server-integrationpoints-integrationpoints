using System;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IQueueManager
	{
		/// <summary>
		/// Determines if there are jobs currently running or queued to run in the given workspace and on the given Integration Point.
		/// Note: This does not include scheduled jobs.
		/// </summary>
		/// <param name="workspaceId">The workspace that the Integration Point belongs to.</param>
		/// <param name="integrationPointId">The Integration Point the job belongs to.</param>
		/// <returns>True if there are jobs running or in queue, false otherwise.</returns>
		bool HasJobsExecutingOrInQueue(int workspaceId, int integrationPointId);

		/// <summary>
		/// Determines if there are any jobs currently executing in the given workspace and on the given Integration Point.
		/// </summary>
		/// <param name="workspaceId">The workspace that the Integration Point belongs to.</param>
		/// <param name="integrationPointId">The Integration Point the job belongs to.</param>
		/// <param name="jobId">The ID of the job in the queue.</param>
		/// <param name="runTime">The timestamp at which the job was kicked off.</param>
		/// <returns>True if there are jobs running, false otherwise.</returns>
		bool HasJobsExecuting(int workspaceId, int integrationPointId, long jobId, DateTime runTime);

		bool HasJobsExecuting(int workspaceId, int integrationPointId);
	}
}
