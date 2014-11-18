using System;
using System.Collections.Generic;
using System.Data;
using kCura.Agent.ScheduleQueueAgent.Helpers;
using kCura.ScheduleQueueAgent.Data;
using kCura.ScheduleQueueAgent.Data.Queries;
using Relativity.API;

namespace kCura.Agent.ScheduleQueueAgent.Services
{
	public class JobService : IJobService
	{
		public JobService(IDBContext dbContext)
		{
			this.QueueTable = new QueueTableHelper().GetQueueTableName();
			this.QDBContext = new QueueDBContext(dbContext, QueueTable);
		}

		public string QueueTable { get; private set; }
		public IQueueDBContext QDBContext { get; private set; }

		public Job GetNextJob(int agentId, IEnumerable<int> resourceGroupIds)
		{
			throw new NotImplementedException();
		}

		public ITask GetTask(Job job)
		{
			throw new NotImplementedException();
		}

		public void FinalizeJob(Job job, TaskResult taskResult)
		{
			throw new NotImplementedException();
		}

		public void UnlockJobs(int agentID)
		{
			throw new NotImplementedException();
		}

		public void CreateQueueTable()
		{
			new CreateScheduleQueueTable(QDBContext).Execute();
		}

		public AgentInformation GetAgentInformation(int agentID)
		{
			AgentInformation agentInformation = null;
			DataRow row = new GetAgentInformation(QDBContext).Execute(agentID);
			if (row != null)
			{
				agentInformation = new AgentInformation(row);
			}
			return agentInformation;
		}
	}
}
