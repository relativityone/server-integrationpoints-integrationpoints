using System.Collections.Generic;
using System.Collections.ObjectModel;
using kCura.Apps.Common.Utils.Serializers;
using Relativity.IntegrationPoints.Tests.Integration.Helpers.RelativityHelpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.Services.Error;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public class RelativityInstanceTest
    {
        private readonly ObservableCollection<WorkspaceTest> _workspaces = new ObservableCollection<WorkspaceTest>();

        public List<Error> Errors { get; } = new List<Error>();

        public List<AgentTest> Agents { get; } = new List<AgentTest>();

        public List<JobTest> JobsInQueue { get; } = new List<JobTest>();

        public List<SyncJobTest> SyncJobsInQueue { get; } = new List<SyncJobTest>();

        public Dictionary<string, List<EntityManagerTest>> EntityManagersResourceTables { get; } = new Dictionary<string, List<EntityManagerTest>>();

        public Dictionary<string, List<JobTrackerTest>> JobTrackerResourceTables { get; } = new Dictionary<string, List<JobTrackerTest>>();

        public IList<WorkspaceTest> Workspaces => _workspaces;

        public TestContext TestContext { get; }

        public IRelativityHelpers Helpers { get; }

        public RelativityInstanceTest(ProxyMock proxy, TestContext testContext, ISerializer serializer)
        {
            TestContext = testContext;
            Helpers = new RelativityHelpers(this, proxy, serializer);
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
            private readonly ISerializer _serializer;
            private WorkspaceHelper _workspaceHelper;
            private AgentHelper _agentHelper;
            private JobHelper _jobHelper;

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
