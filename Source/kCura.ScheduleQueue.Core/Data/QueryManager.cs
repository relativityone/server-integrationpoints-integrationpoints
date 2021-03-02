using System;
using System.Collections.Generic;
using System.Data;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Data.Interfaces;
using kCura.ScheduleQueue.Core.Data.Queries;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Data
{
	public class QueryManager : IQueryManager
	{
		private readonly IQueueDBContext _queueDbContext;

		public IDBContext EddsDbContext => _queueDbContext.EddsDBContext;

		public string QueueTable { get; }

		public QueryManager(IHelper helper, Guid agentGuid)
		{
			QueueTable = $"ScheduleAgentQueue_{agentGuid.ToString().ToUpperInvariant()}";

			_queueDbContext = new QueueDBContext(helper, QueueTable);
		}

		public ICommand CreateScheduleQueueTable()
		{
			return new CreateScheduleQueueTable(_queueDbContext);
		}

		public ICommand AddStopStateColumnToQueueTable()
		{
			return new AddStopStateColumnToQueueTable(_queueDbContext);
		}

		public IQuery<DataRow> GetAgentTypeInformation(Guid agentGuid)
		{
			return new GetAgentTypeInformation(_queueDbContext.EddsDBContext, agentGuid);
		}

		public IQuery<DataTable> GetNextJob(int agentId, int agentTypeId, int[] resourceGroupArtifactId)
		{
			return new GetNextJob(_queueDbContext, agentId, agentTypeId, resourceGroupArtifactId);
		}

		public ICommand UpdateScheduledJob(long jobId, DateTime nextUtcRunTime)
		{
			return new UpdateScheduledJob(_queueDbContext, jobId, nextUtcRunTime);
		}

		public ICommand UnlockScheduledJob(int agentId)
		{
			return new UnlockScheduledJob(_queueDbContext, agentId);
		}

		public ICommand UnlockJob(long jobId)
		{
			return new UnlockJob(_queueDbContext, jobId);
		}

		public ICommand DeleteJob(IDBContext dbContext, string tableName, long jobId)
		{
			return new DeleteJob(dbContext, tableName, jobId);
		}

		public ICommand DeleteJob(long jobId)
		{
			return new DeleteJob(_queueDbContext, jobId);
		}

		public IQuery<DataTable> CreateScheduledJob(IDBContext dbContext, string tableName, int workspaceID, int relatedObjectArtifactID,
			string taskType, DateTime nextRunTime, int AgentTypeID, string scheduleRuleType, string serializedScheduleRule,
			string jobDetails, int jobFlags, int SubmittedBy, long? rootJobID, long? parentJobID = null)
		{
			return new CreateScheduledJob(dbContext, tableName, workspaceID, relatedObjectArtifactID,
				taskType, nextRunTime, AgentTypeID, scheduleRuleType, serializedScheduleRule,
				jobDetails, jobFlags, SubmittedBy, rootJobID, parentJobID);
		}

		public IQuery<DataTable> CreateScheduledJob(int workspaceID, int relatedObjectArtifactID, string taskType, DateTime nextRunTime,
			int AgentTypeID, string scheduleRuleType, string serializedScheduleRule, string jobDetails, int jobFlags,
			int SubmittedBy, long? rootJobID, long? parentJobID = null)
		{
			return new CreateScheduledJob(_queueDbContext, workspaceID, relatedObjectArtifactID,
				taskType, nextRunTime, AgentTypeID, scheduleRuleType, serializedScheduleRule,
				jobDetails, jobFlags, SubmittedBy, rootJobID, parentJobID);
		}

		public ICommand CleanupJobQueueTable()
		{
			return new CleanupJobQueueTable(_queueDbContext);
		}

		public IQuery<DataTable> GetAllJobs()
		{
			return new GetAllJobs(_queueDbContext);
		}

		public IQuery<int> UpdateStopState(IList<long> jobIds, StopState state)
		{
			return new UpdateStopState(_queueDbContext, jobIds, state);
		}

		public IQuery<DataTable> GetJobByRelatedObjectIdAndTaskType(int workspaceId, int relatedObjectArtifactId, List<string> taskTypes)
		{
			return new GetJobByRelatedObjectIdAndTaskType(_queueDbContext, workspaceId, relatedObjectArtifactId, taskTypes);
		}

		public IQuery<DataTable> GetJobsByIntegrationPointId(long integrationPointId)
		{
			return new GetJobsByIntegrationPointId(_queueDbContext, integrationPointId);
		}

		public IQuery<DataTable> GetJob(long jobId)
		{
			return new GetJob(_queueDbContext, jobId);
		}
	}
}
