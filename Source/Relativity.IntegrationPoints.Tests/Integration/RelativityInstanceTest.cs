using System.Collections.Generic;
using System.Collections.ObjectModel;
using kCura.Apps.Common.Utils.Serializers;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration
{
	public class RelativityInstanceTest
	{
		private readonly ProxyMock _proxy;
		private readonly TestContext _testContext;
		private readonly ISerializer _serializer;
		private readonly ObservableCollection<WorkspaceTest> _workspaces = new ObservableCollection<WorkspaceTest>();
		

		public List<AgentTest> Agents { get; set; } = new List<AgentTest>();

		public List<JobTest> JobsInQueue { get; set; } = new List<JobTest>();

		public IList<WorkspaceTest> Workspaces => _workspaces;

		public TestContext TestContext => _testContext;
		
		public IRelativityHelpers Helpers { get; }

		public RelativityInstanceTest(ProxyMock proxy, TestContext testContext, ISerializer serializer)
		{
			_proxy = proxy;
			_testContext = testContext;
			_serializer = serializer;

			SetupWorkspaces();
			Helpers = new RelativityHelpers(this, proxy, _serializer);
		}

		public void Clear()
		{
			Agents.Clear();
			JobsInQueue.Clear();
			Workspaces.Clear();
		}

		private void SetupWorkspaces()
		{
			_workspaces.SetupOnAddedHandler((newItem) =>
			{
				_proxy.ObjectManager.SetupDocumentFields(newItem);
				_proxy.ObjectManager.SetupWorkspace(this, newItem);
			});

			_workspaces.SetupDisposeOnRemove();
		}

		private class RelativityHelpers : IRelativityHelpers
		{
			private readonly RelativityInstanceTest _db;
			private readonly ProxyMock _proxy;
			private WorkspaceHelper _workspaceHelper;
			private AgentHelper _agentHelper;
			private JobHelper _jobHelper;
			private ISerializer _serializer;

			internal RelativityHelpers(RelativityInstanceTest db, ProxyMock proxy, ISerializer serializer)
			{
				_db = db;
				_proxy = proxy;
				_serializer = serializer;
			}
		
			public WorkspaceHelper WorkspaceHelper => _workspaceHelper ?? (_workspaceHelper = new WorkspaceHelper(_db,_proxy, _serializer));
    
			public AgentHelper AgentHelper => _agentHelper ?? (_agentHelper = new AgentHelper(_db));
    
			public JobHelper JobHelper => _jobHelper ?? (_jobHelper = new JobHelper(_db));
		}	
	}
}
