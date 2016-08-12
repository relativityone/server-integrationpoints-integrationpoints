using System;
using System.Data;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Services
{
	public class AgentService : IAgentService
	{
		private bool _creationOfQTableHasRun;

		public AgentService(IHelper dbHelper, Guid agentGuid)
		{
			this.AgentGuid = agentGuid;
			this.QueueTable = string.Format("ScheduleAgentQueue_{0}", agentGuid.ToString().ToUpper());
			this.DBHelper = dbHelper;
			this.QDBContext = new QueueDBContext(dbHelper, QueueTable);
		}

		public Guid AgentGuid { get; }
		public string QueueTable { get; }
		public IHelper DBHelper { get; private set; }
		public IQueueDBContext QDBContext { get; }
		private AgentTypeInformation _agentTypeInformation;

		public AgentTypeInformation AgentTypeInformation
		{
			get
			{
				return _agentTypeInformation ?? (_agentTypeInformation = GetAgentTypeInformation(QDBContext.EddsDBContext, AgentGuid));
			}
		}

		public void InstallQueueTable()
		{
			new CreateScheduleQueueTable(QDBContext).Execute();
			new AddStopStateColumnToQueueTable(QDBContext).Execute();
		}

		public void CreateQueueTableOnce()
		{
			if (!_creationOfQTableHasRun)
			{
				InstallQueueTable();
			}
			_creationOfQTableHasRun = true;
		}

		public static AgentTypeInformation GetAgentTypeInformation(IDBContext eddsDBContext, Guid agentGuid)
		{
			DataRow row = new GetAgentTypeInformation(eddsDBContext).Execute(agentGuid);
			if (row == null)
			{
				throw new AgentNotFoundException(String.Format("The agent with Guid {0} could not be found, please ensure there is an existing installed agent", agentGuid.ToString()));
			}

			var agentTypeInformation = new AgentTypeInformation(row);
			return agentTypeInformation;
		}
	}
}