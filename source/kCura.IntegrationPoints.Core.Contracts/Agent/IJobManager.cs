﻿using System;
using System.Collections.Generic;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Core.Contracts.Agent
{
	public enum TaskType
	{
		None,
		SyncManager,
		SyncWorker,
		SyncCustodianManagerWorker,
		SendEmailManager,
		SendEmailWorker,
		ExportService,
        ExportManager,
		ExportWorker
	}

	public interface IJobManager
	{
		void CreateJobOnBehalfOfAUser<T>(T jobDetails, TaskType task, int workspaceId, int integrationPointId, int userId, long? rootJobId = null, long? parentJobId = null);
		void CreateJob<T>(T jobDetails, TaskType task, int workspaceId, int integrationPointId, IScheduleRule rule, long? rootJobID = null, long? parentJobID = null);
		void CreateJob<T>(T jobDetails, TaskType task, int workspaceId, int integrationPointId, long? rootJobId = null, long? parentJobId = null);
		void CreateJob<T>(Job parentJob, T jobDetails, TaskType task);
		void DeleteJob(long jobID);
		Job GetJob(int workspaceID, int relatedObjectArtifactID, string taskName);
		void CreateJobWithTracker<T>(Job parentJob, T jobDetails, TaskType type, string batchId);
		bool CheckBatchOnJobComplete(Job job, string batchId);

		/// <summary>
		/// Stop the scheduled queue job
		/// </summary>
		/// <param name="jobId">A scheduled queue job id</param>
		void StopJob(long jobId);

		/// <summary>
		/// Get scheduled agent jobs as a dictionary where the key is the job history's batch instance id and the value is a list of scheduled agent job dtos.
		/// </summary>
		/// <param name="integrationPointId">An artifact id of integration point object.</param>
		/// <returns>A dictionary of batch instance id and its agent job DTOs.</returns>
		IDictionary<Guid, List<Job>> GetScheduledAgentJobMapedByBatchInstance(long integrationPointId);
	}
}
