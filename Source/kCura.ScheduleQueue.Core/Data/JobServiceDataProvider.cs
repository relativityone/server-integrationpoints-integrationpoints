﻿using System;
using System.Collections.Generic;
using System.Data;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data.Interfaces;
using kCura.ScheduleQueue.Core.Data.Queries;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Data
{
	public class JobServiceDataProvider : IJobServiceDataProvider
	{
		private readonly IQueryManager _queryManager;

		public JobServiceDataProvider(IQueryManager queryManager)
		{
			_queryManager = queryManager;
		}

		public DataRow GetNextQueueJob(int agentId, int agentTypeId, int[] resourceGroupIdsArray)
		{
			using (DataTable dataTable = _queryManager.GetNextJob(agentId, agentTypeId, resourceGroupIdsArray).Execute())
			{
				return GetFirstRowOrDefault(dataTable);
			}
		}

		private DataRow GetFirstRowOrDefault(DataTable dataTable)
		{
			return dataTable?.Rows?.Count > 0 ? dataTable.Rows[0] : null;
		}

		public void UpdateScheduledJob(long jobId, DateTime nextUtcRunDateTime)
		{
			_queryManager
				.UpdateScheduledJob(jobId, nextUtcRunDateTime)
				.Execute();
		}

		public void UnlockScheduledJob(int agentId)
		{
			_queryManager
				.UnlockScheduledJob(agentId)
				.Execute();
		}

		public void UnlockJob(long jobID)
		{
			_queryManager
				.UnlockJob(jobID)
				.Execute();
		}

		public void CreateNewAndDeleteOldScheduledJob(long oldScheduledJobId, int workspaceID, int relatedObjectArtifactID, string taskType,
			DateTime nextRunTime, int agentTypeId, string scheduleRuleType, string serializedScheduleRule,
			string jobDetails, int jobFlags, int submittedBy, long? rootJobID, long? parentJobID)
		{
			IDBContext dbContext = _queryManager.EddsDbContext;
			try
			{
				dbContext.BeginTransaction();

				DeleteJob(oldScheduledJobId, _queryManager.QueueTable, dbContext);

				CreateScheduledJob(workspaceID, relatedObjectArtifactID, taskType,
					nextRunTime, agentTypeId, scheduleRuleType, serializedScheduleRule,
					jobDetails, jobFlags, submittedBy, rootJobID, parentJobID, _queryManager.QueueTable, dbContext);

				dbContext.CommitTransaction();
			}
			catch (Exception )
			{
				dbContext.RollbackTransaction();
				throw;
			}
			
		}

		public DataRow CreateScheduledJob(int workspaceID, int relatedObjectArtifactID, string taskType,
			DateTime nextRunTime, int agentTypeId, string scheduleRuleType, string serializedScheduleRule,
			string jobDetails, int jobFlags, int submittedBy, long? rootJobID, long? parentJobID)
		{
			using (DataTable dataTable = _queryManager.CreateScheduledJob(
				workspaceID,
				relatedObjectArtifactID,
				taskType,
				nextRunTime,
				agentTypeId,
				scheduleRuleType,
				serializedScheduleRule,
				jobDetails,
				jobFlags,
				submittedBy,
				rootJobID,
				parentJobID).Execute())
			{
				return GetFirstRowOrDefault(dataTable);
			}
		}

		public DataTable GetJobsByIntegrationPointId(long integrationPointId)
		{
			return _queryManager
				.GetJobsByIntegrationPointId(integrationPointId)
				.Execute();
		}

		public void DeleteJob(long jobId)
		{
			_queryManager
				.DeleteJob(jobId)
				.Execute();
		}

		public void DeleteJob(long jobId, string tableName, IDBContext context)
		{
			_queryManager
				.DeleteJob(context, tableName, jobId)
				.Execute();
		}

		public DataRow GetJob(long jobId)
		{
			using (DataTable dataTable = _queryManager.GetJob(jobId).Execute())
			{
				return GetFirstRowOrDefault(dataTable);
			}
		}

		public DataTable GetJobs(int workspaceId, int relatedObjectArtifactId, string taskType)
		{
			return GetJobs(workspaceId, relatedObjectArtifactId, new List<string> {taskType});
		}

		public DataTable GetJobs(int workspaceId, int relatedObjectArtifactId, List<string> taskTypes)
		{
			return _queryManager
				.GetJobByRelatedObjectIdAndTaskType(workspaceId, relatedObjectArtifactId, taskTypes)
				.Execute();
		}

		public DataTable GetAllJobs()
		{
			return _queryManager
				.GetAllJobs()
				.Execute();
		}

		public int UpdateStopState(IList<long> jobIds, StopState state)
		{
			return _queryManager
				.UpdateStopState(jobIds, state)
				.Execute();
		}

		public void CleanupJobQueueTable()
		{
			_queryManager
				.CleanupJobQueueTable()
				.Execute();
		}

		private DataRow CreateScheduledJob(int workspaceID, int relatedObjectArtifactID, string taskType,
			DateTime nextRunTime, int agentTypeId, string scheduleRuleType, string serializedScheduleRule,
			string jobDetails, int jobFlags, int submittedBy, long? rootJobID, long? parentJobID, 
			string tableName, IDBContext dbContext)
		{
			using (DataTable dataTable = _queryManager.CreateScheduledJob(dbContext, tableName,
				workspaceID,
				relatedObjectArtifactID,
				taskType,
				nextRunTime,
				agentTypeId,
				scheduleRuleType,
				serializedScheduleRule,
				jobDetails,
				jobFlags,
				submittedBy,
				rootJobID,
				parentJobID).Execute())
			{
				return GetFirstRowOrDefault(dataTable);
			}
		}
	}
}