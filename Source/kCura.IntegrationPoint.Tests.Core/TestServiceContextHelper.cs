using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
    public class TestServiceContextHelper : IServiceContextHelper
    {
        private readonly IHelper _helper;
        private readonly IDbContextFactory _dbContextFactory;

        public TestServiceContextHelper(IHelper helper, int workspaceArtifactId)
        {
            _helper = helper;
            WorkspaceID = workspaceArtifactId;
            _dbContextFactory = new DbContextFactory(helper);
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

        public IRipDBContext GetDBContext(int workspaceID = -1)
        {
            return _dbContextFactory.CreateWorkspaceDbContext(workspaceID);
        }

        public IRelativityObjectManagerService GetRelativityObjectManagerService()
        {
            return ServiceContextFactory.CreateRelativityObjectManagerService(_helper, WorkspaceID);
        }
    }
}
