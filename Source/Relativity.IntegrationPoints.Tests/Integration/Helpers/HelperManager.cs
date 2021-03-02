using Relativity.IntegrationPoints.Tests.Integration.Mocks;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class HelperManager
	{
		private readonly InMemoryDatabase _db;
		private readonly ProxyMock _proxy;

		#region Helper Fields

		private WorkspaceHelper _workspaceHelper;

		private AgentHelper _agentHelper;

		private JobHelper _jobHelper;

		private IntegrationPointHelper _integrationPointHelper;


		#endregion

		public HelperManager(InMemoryDatabase db, ProxyMock proxy)
		{
			_db = db;
			_proxy = proxy;
		}

		public WorkspaceHelper WorkspaceHelper => _workspaceHelper ?? (_workspaceHelper = new WorkspaceHelper(_db, _proxy));

		public AgentHelper AgentHelper => _agentHelper ?? (_agentHelper = new AgentHelper(_db, _proxy));

		public JobHelper JobHelper => _jobHelper ?? (_jobHelper = new JobHelper(_db, _proxy));

		public IntegrationPointHelper IntegrationPointHelper => _integrationPointHelper ?? (_integrationPointHelper = new IntegrationPointHelper(_db, _proxy));
	}
}
