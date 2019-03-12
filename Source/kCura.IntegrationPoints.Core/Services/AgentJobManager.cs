﻿using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Properties;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services
{
	public class AgentJobManager : IJobManager
	{
		private readonly IEddsServiceContext _context;
		private readonly IJobService _jobService;
		private readonly IAPILog _logger;
		private readonly IIntegrationPointSerializer _serializer;
		private readonly IJobTracker _tracker;

		public AgentJobManager(IEddsServiceContext context, IJobService jobService, IHelper helper, IIntegrationPointSerializer serializer, IJobTracker tracker)
		{
			_context = context;
			_jobService = jobService;
			_serializer = serializer;
			_tracker = tracker;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<AgentJobManager>();
		}

		public void CreateJob<T>(T jobDetails, TaskType task, int workspaceId, int integrationPointId, IScheduleRule rule, long? rootJobID = null, long? parentJobID = null)
		{
			try
			{
				string serializedDetails = null;
				if(!Equals(jobDetails, default(T)))
				{
					serializedDetails = _serializer.Serialize(jobDetails);
				}
				if (rule != null)
				{
					_jobService.CreateJob(workspaceId, integrationPointId, task.ToString(), rule, serializedDetails, _context.UserID, rootJobID, parentJobID);
				}
				else
				{
					_jobService.CreateJob(workspaceId, integrationPointId, task.ToString(), DateTime.UtcNow, serializedDetails, _context.UserID, rootJobID, parentJobID);
				}
			}
			catch (AgentNotFoundException anfe)
			{
				LogCreatingJobError(anfe, task, workspaceId, integrationPointId);
				throw new Exception(ErrorMessages.NoAgentInstalled, anfe);
			}
		}

		public void CreateJob<T>(Job parentJob, T jobDetails, TaskType task)
		{
			CreateJob(jobDetails, task, parentJob.WorkspaceID, parentJob.RelatedObjectArtifactID, GetRootJobId(parentJob), parentJob.JobId);
		}

		public void CreateJobWithTracker<T>(Job parentJob, T jobDetails, TaskType type, string batchId)
		{
			Job job = CreateJobInternal(jobDetails, type, parentJob.WorkspaceID, parentJob.RelatedObjectArtifactID, parentJob.SubmittedBy, GetRootJobId(parentJob), parentJob.JobId);
			_tracker.CreateTrackingEntry(job, batchId);
		}

		public bool CheckBatchOnJobComplete(Job job, string batchId)
		{
			return _tracker.CheckEntries(job, batchId);
		}

		public void CreateJob<T>(T jobDetails, TaskType task, int workspaceId, int integrationPointId, long? rootJobId = null, long? parentJobId = null)
		{
			CreateJobInternal(jobDetails, task, workspaceId, integrationPointId, _context.UserID, rootJobId, parentJobId);
		}

		public void CreateJobOnBehalfOfAUser<T>(T jobDetails, TaskType task, int workspaceId, int integrationPointId, int userId, long? rootJobId = null,
			long? parentJobId = null)
		{
			CreateJobInternal(jobDetails, task, workspaceId, integrationPointId, userId, rootJobId, parentJobId);
		}

		public Job GetJob(int workspaceID, int relatedObjectArtifactID, string taskName)
		{
			return _jobService.GetScheduledJobs(workspaceID, relatedObjectArtifactID, taskName);
		}

		public void DeleteJob(long jobID)
		{
			try
			{
				_jobService.DeleteJob(jobID);
			}
			catch (AgentNotFoundException anfe)
			{
				LogDeletingJobError(jobID, anfe);
				throw new Exception(ErrorMessages.NoAgentInstalled, anfe);
			}
		}

		public IDictionary<Guid, List<Job>> GetScheduledAgentJobMapedByBatchInstance(long integrationPointId)
		{
			IDictionary<Guid, List<Job>> results = new Dictionary<Guid, List<Job>>();
			IList<Job> jobs = _jobService.GetJobs(integrationPointId);
			foreach (var job in jobs)
			{
				try
				{
					TaskParameters parameter = _serializer.Deserialize<TaskParameters>(job.JobDetails);
					if (results.ContainsKey(parameter.BatchInstance))
					{
						results[parameter.BatchInstance].Add(job);
					}
					else
					{
						results[parameter.BatchInstance] = new List<Job> {job};
					}
				}
				catch(Exception e)
				{
					LogTaskParametersDeserializationError(e);
					// in case of the serialization fails for whatever reasons.
				}
			}
			return results;
		}

		public IList<Job> GetJobsByBatchInstanceId(long integrationPointId, Guid batchId)
		{
			IDictionary<Guid, List<Job>> bacthedAgentJobs = GetScheduledAgentJobMapedByBatchInstance(integrationPointId);
			if (!bacthedAgentJobs.ContainsKey(batchId))
			{
				LogFailedToFindBatchInstance(integrationPointId, batchId);
				throw new Exception("Unable to find the batch instance id in the scheduled agent queue.");
			}
			return bacthedAgentJobs[batchId];
		}


		public void StopJobs(IList<long> jobIds)
		{
			_jobService.UpdateStopState(jobIds, StopState.Stopping);
		}

		private Job CreateJobInternal<T>(T jobDetails, TaskType task, int workspaceId, int integrationPointId, int userId, long? rootJobId = null, long? parentJobID = null)
		{
			try
			{
				string serializedDetails = null;
				if (!Equals(jobDetails, default(T)))
				{
					serializedDetails = _serializer.Serialize(jobDetails);
				}
				return _jobService.CreateJob(workspaceId, integrationPointId, task.ToString(), DateTime.UtcNow, serializedDetails, userId, rootJobId, parentJobID);
			}
			catch (AgentNotFoundException anfe)
			{
				LogCreatingJobError(anfe, task, workspaceId, integrationPointId);
				throw new Exception(ErrorMessages.NoAgentInstalled, anfe);
			}
		}

		public void CreateJob(int workspaceID, int integrationPointID, TaskType task, string serializedDetails, long? rootJobId = null, long? parentJobId = null)
		{
			try
			{
				_jobService.CreateJob(workspaceID, integrationPointID, task.ToString(), DateTime.UtcNow, serializedDetails, _context.UserID, rootJobId, parentJobId);
			}
			catch (AgentNotFoundException anfe)
			{
				LogCreatingJobError(anfe, task, workspaceID, integrationPointID);
				throw new Exception(ErrorMessages.NoAgentInstalled, anfe);
			}
		}

		public static long? GetRootJobId(Job parentJob)
		{
			long? rootJobId = parentJob.RootJobId;

			if (!rootJobId.HasValue)
			{
				rootJobId = parentJob.JobId;
			}

			return rootJobId;
		}

		#region Logging

		private void LogCreatingJobError(AgentNotFoundException anfe, TaskType task, int workspaceId, int integrationPointId)
		{
			_logger.LogError(anfe, "Failed to create job of type {TaskType} for Integration Point {IntegrationPointId} in Workspace {WorkspaceId}.", task, integrationPointId,
				workspaceId);
		}

		private void LogDeletingJobError(long jobID, AgentNotFoundException anfe)
		{
			_logger.LogError(anfe, "Failed to delete job {JobId}.", jobID);
		}

		private void LogTaskParametersDeserializationError(Exception e)
		{
			_logger.LogError(e, "Failed to deserialize TaskParameters.");
		}

		private void LogFailedToFindBatchInstance(long integrationPointId, Guid batchId)
		{
			_logger.LogError("Unable to find the batch instance id {BatchId} in the scheduled agent queue for Integration Point {IPId}.", batchId.ToString(), integrationPointId);
		}

		#endregion
	}
}