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
		private readonly IAPILog _logger;

		public AgentService(IHelper dbHelper, Guid agentGuid)
		{
			AgentGuid = agentGuid;
			QueueTable = string.Format("ScheduleAgentQueue_{0}", agentGuid.ToString().ToUpper());
			DBHelper = dbHelper;
			QDBContext = new QueueDBContext(dbHelper, QueueTable);
			_logger = dbHelper.GetLoggerFactory().GetLogger().ForContext<AgentService>();
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
			LogInstallQueueTable();
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
				string message = $"The agent with Guid {agentGuid} could not be found, please ensure there is an existing installed agent";
				throw new AgentNotFoundException(message);
			}

			var agentTypeInformation = new AgentTypeInformation(row);
			return agentTypeInformation;
		}

		#region Logging

		private void LogInstallQueueTable()
		{
			_logger.LogInformation("Attepting to create ScheduleQueue table");
		}

		#endregion
	}
}