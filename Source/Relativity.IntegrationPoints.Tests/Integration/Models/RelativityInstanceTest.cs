﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using kCura.Apps.Common.Utils.Serializers;
using Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class RelativityInstanceTest
	{
		private readonly TestContext _testContext;
		private readonly ISerializer _serializer;
		private readonly ObservableCollection<WorkspaceTest> _workspaces = new ObservableCollection<WorkspaceTest>();
		

		public List<AgentTest> Agents { get; } = new List<AgentTest>();

		public List<JobTest> JobsInQueue { get; } = new List<JobTest>();

		public IList<WorkspaceTest> Workspaces => _workspaces;

		public TestContext TestContext => _testContext;
		
		public IRelativityHelpers Helpers { get; }

		public RelativityInstanceTest(ProxyMock proxy, TestContext testContext, ISerializer serializer)
		{
			_testContext = testContext;
			_serializer = serializer;

			Helpers = new RelativityHelpers(this, proxy, _serializer);
		}

		public void Clear()
		{
			Agents.Clear();
			JobsInQueue.Clear();
			Workspaces.Clear();
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
