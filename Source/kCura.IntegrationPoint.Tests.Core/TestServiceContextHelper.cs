using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
    public class TestServiceContextHelper : IServiceContextHelper
    {
        private readonly IHelper _helper;

        public TestServiceContextHelper(IHelper helper, int workspaceArtifactId)
        {
            _helper = helper;
            WorkspaceID = workspaceArtifactId;
        }

        public int WorkspaceID { get; }

        public int GetEddsUserID()
        {
            return 9;
        }

        public int GetWorkspaceUserID()
        {
            return 9;
        }

        public IDBContext GetDBContext(int workspaceID = -1)
        {
            return _helper.GetDBContext(workspaceID);
        }

        public IRelativityObjectManagerService GetRelativityObjectManagerService()
        {
            return ServiceContextFactory.CreateRelativityObjectManagerService(_helper, WorkspaceID);
        }
    }
}