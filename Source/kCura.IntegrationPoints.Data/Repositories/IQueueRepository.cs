using System;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IQueueRepository
	{
		/// <summary>
		/// Queries the ScheduleAgentQueue table for the number of jobs that belong to the same workspace and Integration Point that are currently in the queue or running. 
		/// NOTE: Scheduled jobs are intentionally excluded from the result..
		/// </summary>
		/// <param name="workspaceId">The Artifact ID of the workspace that the job is run from.</param>
		/// <param name="integrationPointId">The Artifact ID of the Integration Point the job belongs to.</param>
		/// <returns>The number of jobs actively running or queued up to run (excluding scheduled jobs).</returns>
		int GetNumberOfJobsExecutingOrInQueue(int workspaceId, int integrationPointId);

		/// <summary>
		/// Queries the ScheduleAgentQueue table for the number of jobs that belong to the same workspace and Integration Point that are currently running.
		/// </summary>
		/// <param name="workspaceId">The Artifact ID of the workspace that the job is run from.</param>
		/// <param name="integrationPointId">The Artifact ID of the Integration Point the job belongs to.</param>
		/// <param name="jobId">The ID of the Job that is attempting to run, we exclude this from the results of the query.</param>
		/// <param name="runTime">The run time of the Job attempting to be run.</param>
		/// <returns>The number of jobs actively running.</returns>
		int GetNumberOfJobsExecuting(int workspaceId, int integrationPointId, long jobId, DateTime runTime);

		/// <summary>
		/// Queries the ScheduleAgentQueue table for the number of jobs that belong to the same workspace and Integration Point that are currently in the queue. 
		/// </summary>
		/// <param name="workspaceId">The Artifact ID of the workspace that the job is run from.</param>
		/// <param name="integrationPointId">The Artifact ID of the Integration Point the job belongs to.</param>
		/// <returns>The number of jobs that are queued up to run.</returns>
		int GetNumberOfPendingJobs(int workspaceId, int integrationPointId);

		int GetNumberOfJobsLockedByAgentForIntegrationPoint(int workspaceId, int integrationPointId);
	}
}
