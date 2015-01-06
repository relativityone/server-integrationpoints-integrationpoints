using System;
using System.Data;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Services
{
	public class AgentService : IAgentService
	{
		private bool creationOfQTableHasRun = false;
		public AgentService(IDBContext dbContext, Guid agentGuid)
		{
			this.AgentGuid = agentGuid;
			this.QueueTable = string.Format("ScheduleAgentQueue_{0}", agentGuid.ToString().ToUpper());
			this.AgentInformation = AgentService.GetAgentInformation(dbContext, agentGuid);
			this.QDBContext = new QueueDBContext(dbContext, QueueTable);
		}

		public AgentService()
		{
			
		}
		public Guid AgentGuid { get; private set; }
		public string QueueTable { get; private set; }
		public IQueueDBContext QDBContext { get; private set; }
		public AgentInformation AgentInformation { get; private set; }

		public void CreateQueueTable()
		{
			new CreateScheduleQueueTable(QDBContext).Execute();
		}

		public void CreateQueueTableOnce()
		{
			if (!creationOfQTableHasRun) CreateQueueTable();
			creationOfQTableHasRun = true;
		}

		public static AgentInformation GetAgentInformation(IDBContext dbContext, int agentID)
		{
			AgentInformation agentInformation = null;
			DataRow row = new GetAgentInformation(dbContext).Execute(agentID);
			if (row != null)
			{
				agentInformation = new AgentInformation(row);
			}
			return agentInformation;
		}

		public static AgentInformation GetAgentInformation(IDBContext dbContext, Guid agentGuid)
		{
			AgentInformation agentInformation = null;
			DataRow row = new GetAgentInformation(dbContext).Execute(agentGuid);
			if (row != null)
			{
				agentInformation = new AgentInformation(row);
			}
			return agentInformation;
		}
	}
}
