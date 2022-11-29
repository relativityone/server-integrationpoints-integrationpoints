using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
    public class ServiceContextHelperForKeplerService : IServiceContextHelper
    {
        private readonly IServiceHelper _helper;
        private readonly IDbContextFactory _dbContextFactory;

        public ServiceContextHelperForKeplerService(IServiceHelper helper, int workspaceArtifactId)
        {
            _helper = helper;
            WorkspaceID = workspaceArtifactId;
            _dbContextFactory = new DbContextFactory(helper);

        }

        public int WorkspaceID { get; }

        public int GetEddsUserID()
        {
            return _helper.GetAuthenticationManager().UserInfo.ArtifactID;
        }

        public int GetWorkspaceUserID()
        {
            return _helper.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID;
        }

        public IRipDBContext GetDBContext(int workspaceId = -1)
        {
            return _dbContextFactory.CreateWorkspaceDbContext(workspaceId);
        }

        public IRelativityObjectManagerService GetRelativityObjectManagerService()
        {
            return ServiceContextFactory.CreateRelativityObjectManagerService(_helper, WorkspaceID);
        }
    }
}
