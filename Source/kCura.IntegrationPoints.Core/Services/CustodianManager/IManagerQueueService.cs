using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.CustodianManager;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services.CustodianManager
{
	public interface IManagerQueueService
	{
		/// <summary>
		/// Gets the RIP_CustodianManager temp table name based on job parameters
		/// </summary>
		/// <param name="job">Job instance to retrieve the correct temp table name for</param>
		/// <param name="batchInstance">Job batch instance to retrieve the correct temp table name for</param>
		/// <returns>The temp table name for the provided job details</returns>
		string GetTempTableName(Job job, Guid batchInstance);

		/// <summary>
		/// Gets the RIP_CustodianManager temp table name based on job parameters
		/// </summary>
		/// <param name="workspaceId">The workspace artifact id the job belongs to</param>
		/// <param name="relatedObjectArtifactId">The integration point artifact id the job is related to</param>
		/// <param name="batchInstance">Job batch instance to retrieve the correct temp table name for</param>
		/// <returns>The temp table name for the provided job details</returns>
		string GetTempTableName(int workspaceId, int relatedObjectArtifactId, Guid batchInstance);

		/// <summary>
		/// Returns a list of Custodian Manager mapping information to be processed
		/// </summary>
		/// <param name="job">Job instance to associate the temp table with to store CustodianManagerMap information</param>
		/// <param name="batchInstance">Job batch instance to associate the temp table with to store CustodianManagerMap information</param>
		/// <param name="jobCustodianManagerMap">Custodian Manager mapping information to be inserted to the temp table and processed</param>
		/// <returns>A list of Custodian Manager mapping information to be processed</returns>
		List<CustodianManagerMap> GetCustodianManagerLinksToProcess(Job job, Guid batchInstance,
			List<CustodianManagerMap> jobCustodianManagerMap);

		/// <summary>
		/// Checks the count of associated jobs to decide whether all batches are completed
		/// </summary>
		/// <param name="job">Job instance to check for the associated job count</param>
		/// <param name="taskTypeExceptions">Task types to exclude when checking for the count of associated jobs</param>
		/// <returns>True if associated batch jobs are complete, false otherwise</returns>
		bool AreAllTasksOfTheBatchDone(Job job, string[] taskTypeExceptions);
	}
}