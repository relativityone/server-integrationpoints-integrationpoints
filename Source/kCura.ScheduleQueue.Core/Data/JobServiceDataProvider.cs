using System;
using System.Collections.Generic;
using System.Data;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data.Queries;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Data
{
	public class JobServiceDataProvider : IJobServiceDataProvider
	{
		private readonly IQueueDBContext _context;

		public JobServiceDataProvider(IAgentService agentService, IHelper dbHelper)
		{
			_context = new QueueDBContext(dbHelper, agentService.QueueTable);
		}

		public DataRow GetNextQueueJob(int agentId, int agentTypeId, int[] resurceGroupIdsArray)
		{
			var query = new GetNextJob(_context);

			using (DataTable dataTable = query.Execute(agentId, agentTypeId, resurceGroupIdsArray))
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
			new UpdateScheduledJob(_context).Execute(jobId, nextUtcRunDateTime);
		}

		public void UnlockScheduledJob(int agentId)
		{
			new UnlockScheduledJob(_context).Execute(agentId);
		}

		public DataRow CreateScheduledJob(int workspaceID, int relatedObjectArtifactID, string taskType,
			DateTime nextRunTime, int agentTypeId, string scheduleRuleType, string serializedScheduleRule,
			string jobDetails, int jobFlags, int submittedBy, long? rootJobID, long? parentJobID)
		{
			var query = new CreateScheduledJob(_context);
			using (DataTable dataTable = query.Execute(
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
				parentJobID))
			{
				return GetFirstRowOrDefault(dataTable);
			}
		}

		public DataTable GetJobsByIntegrationPointId(long integrationPointId)
		{
			var query = new GetJobsByIntegrationPointId(_context);
			return query.Execute(integrationPointId);
		}

		public void DeleteJob(long jobId)
		{
			new DeleteJob(_context).Execute(jobId);
		}

		public DataRow GetJob(long jobId)
		{
			using (DataTable dataTable = new GetJob(_context).Execute(jobId))
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
			return new GetJobByRelatedObjectIdAndTaskType(_context).Execute(workspaceId, relatedObjectArtifactId, taskTypes);
		}

		public DataTable GetAllJobs()
		{
			return new GetAllJobs(_context).Execute();
		}

		public int UpdateStopState(IList<long> jobIds, StopState state)
		{
			return new UpdateStopState(_context).Execute(jobIds, state);
		}

		public void CleanupJobQueueTable()
		{
			new CleanupJobQueueTable(_context).Execute();
		}
	}
}