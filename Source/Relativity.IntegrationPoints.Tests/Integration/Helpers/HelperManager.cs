using Relativity.IntegrationPoints.Tests.Integration.Mocks;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class HelperManager
	{
		private readonly InMemoryDatabase _db;
		private readonly ProxyMock _proxy;
		
		private WorkspaceHelper _workspaceHelper;

		private AgentHelper _agentHelper;

		private JobHelper _jobHelper;

		private IntegrationPointHelper _integrationPointHelper;

		private IntegrationPointTypeHelper _integrationPointTypeHelper;

		private JobHistoryHelper _jobHistoryHelper;

		private SourceProviderHelper _sourceProviderHelper;

		private DestinationProviderHelper _destinationProviderHelper;

		public HelperManager(InMemoryDatabase db, ProxyMock proxy)
		{
			_db = db;
			_proxy = proxy;
		}

		public WorkspaceHelper WorkspaceHelper => _workspaceHelper ?? (_workspaceHelper = new WorkspaceHelper(this, _db, _proxy));

		public AgentHelper AgentHelper => _agentHelper ?? (_agentHelper = new AgentHelper(this, _db, _proxy));

		public JobHelper JobHelper => _jobHelper ?? (_jobHelper = new JobHelper(this, _db, _proxy));

		public IntegrationPointHelper IntegrationPointHelper => _integrationPointHelper ?? (_integrationPointHelper = new IntegrationPointHelper(this, _db, _proxy));
		
		public IntegrationPointTypeHelper IntegrationPointTypeHelper => _integrationPointTypeHelper ?? (_integrationPointTypeHelper = new IntegrationPointTypeHelper(this, _db, _proxy));

		public JobHistoryHelper JobHistoryHelper => _jobHistoryHelper ?? (_jobHistoryHelper = new JobHistoryHelper(this, _db, _proxy));

		public SourceProviderHelper SourceProviderHelper => _sourceProviderHelper ?? (_sourceProviderHelper = new SourceProviderHelper(this, _db, _proxy));

		public DestinationProviderHelper DestinationProviderHelper => _destinationProviderHelper ?? (_destinationProviderHelper = new DestinationProviderHelper(this, _db, _proxy));
	}
}
