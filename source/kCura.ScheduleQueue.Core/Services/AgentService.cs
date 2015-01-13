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
		private IHelper _dbHelper;
		public AgentService(IHelper dbHelper, Guid agentGuid)
		{
			this.AgentGuid = agentGuid;
			this.QueueTable = string.Format("ScheduleAgentQueue_{0}", agentGuid.ToString().ToUpper());
			this.DBHelper = dbHelper;
			this.QDBContext = new QueueDBContext(dbHelper, QueueTable);

		}
		
		public Guid AgentGuid { get; private set; }
		public string QueueTable { get; private set; }
		public IHelper DBHelper { get; private set; }
		public IQueueDBContext QDBContext { get; private set; }
		private AgentInformation _agentInformation;
		public AgentInformation AgentInformation
		{
			get { return _agentInformation ?? (_agentInformation = GetAgentInformation(QDBContext.EddsDBContext, AgentGuid)); }

		}

		public void CreateQueueTable()
		{
			new CreateScheduleQueueTable(QDBContext).Execute();
		}

		public void CreateQueueTableOnce()
		{
			if (!creationOfQTableHasRun) CreateQueueTable();
			creationOfQTableHasRun = true;
		}

		public static AgentInformation GetAgentInformation(IDBContext eddsDBContext, int agentID)
		{
			AgentInformation agentInformation = null;
			DataRow row = new GetAgentInformation(eddsDBContext).Execute(agentID);
			if (row == null)
			{
				throw new AgentNotFoundException(string.Format("The agent with agentID {0} could not be found, please ensure there is an existing installed agent", agentID));
			}
			agentInformation = new AgentInformation(row);
			return agentInformation;
		}

		public static AgentInformation GetAgentInformation(IDBContext eddsDBContext, Guid agentGuid)
		{
			AgentInformation agentInformation = null;
			DataRow row = new GetAgentInformation(eddsDBContext).Execute(agentGuid);
			if (row == null)
			{
				throw new AgentNotFoundException(string.Format("The agent with Guid {0} could not be found, please ensure there is an existing installed agent", agentGuid.ToString()));
			}
			agentInformation = new AgentInformation(row);

			return agentInformation;
		}
	}
}
