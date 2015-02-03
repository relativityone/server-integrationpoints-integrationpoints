﻿using System;
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
		private AgentTypeInformation _agentTypeInformation;
		public AgentTypeInformation AgentTypeInformation
		{
			get { return _agentTypeInformation ?? (_agentTypeInformation = GetAgentTypeInformation(QDBContext.EddsDBContext, AgentGuid)); }

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

		public static AgentTypeInformation GetAgentTypeInformation(IDBContext eddsDBContext, Guid agentGuid)
		{
			AgentTypeInformation agentTypeInformation = null;
			DataRow row = new GetAgentTypeInformation(eddsDBContext).Execute(agentGuid);
			if (row == null)
			{
				throw new AgentNotFoundException(string.Format("The agent with Guid {0} could not be found, please ensure there is an existing installed agent", agentGuid.ToString()));
			}
			agentTypeInformation = new AgentTypeInformation(row);

			return agentTypeInformation;
		}
	}
}
