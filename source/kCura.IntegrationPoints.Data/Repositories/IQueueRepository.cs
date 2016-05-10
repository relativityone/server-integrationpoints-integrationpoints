using System;

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
		/// <returns>The number of jobs actively running or queued up to run (excluding scheduled jobs).</returns>
		int GetNumberOfJobsExecutingOrInQueue(int workspaceId, int integrationPointId);

		/// <summary>
		/// Queries the ScheduleAgentQueue table to determine if there are any jobs that belong to the same workspace and Integration Point currently running.
		/// </summary>
		/// <param name="workspaceId">The Artifact ID of the workspace that the job is run from.</param>
		/// <param name="integrationPointId">The Artifact ID of the Integration Point the job belongs to.</param>
		/// <param name="jobId"></param>
		/// <param name="runTime"></param>
		/// <returns>The number of jobs actively running.</returns>
		int GetNumberOfJobsExecuting(int workspaceId, int integrationPointId, long jobId, DateTime runTime);
	}
}
