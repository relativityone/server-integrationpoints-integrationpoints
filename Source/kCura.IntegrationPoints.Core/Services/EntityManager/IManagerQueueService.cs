using System;
using System.Collections.Generic;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services.EntityManager
{
	public interface IManagerQueueService
	{
		/// <summary>
		/// Returns a list of Entity Manager mapping information to be processed
		/// </summary>
		/// <param name="job">Job instance to associate the temp table with to store EntityManagerMap information</param>
		/// <param name="batchInstance">Job batch instance to associate the temp table with to store EntityManagerMap information</param>
		/// <param name="entityManagerMap">Entity Manager mapping information to be inserted to the temp table and processed</param>
		/// <returns>A list of Entity Manager mapping information to be processed</returns>
		List<EntityManagerMap> GetEntityManagerLinksToProcess(Job job, Guid batchInstance,
			List<EntityManagerMap> entityManagerMap);

		/// <summary>
		/// Checks the count of associated jobs to decide whether all batches are completed
		/// </summary>
		/// <param name="job">Job instance to check for the associated job count</param>
		/// <param name="taskTypeExceptions">Task types to exclude when checking for the count of associated jobs</param>
		/// <returns>True if associated batch jobs are complete, false otherwise</returns>
		bool AreAllTasksOfTheBatchDone(Job job, string[] taskTypeExceptions);

		void MarkEntityManagerLinksAsExpired(Job job, Guid batchInstance);
	}
}