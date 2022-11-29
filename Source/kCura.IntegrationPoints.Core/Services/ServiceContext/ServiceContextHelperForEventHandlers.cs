using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
    public class ServiceContextHelperForEventHandlers : IServiceContextHelper
    {
        private readonly IEHHelper _helper;
        private readonly IDbContextFactory _dbContextFactory;

        public ServiceContextHelperForEventHandlers(IEHHelper helper, int workspaceId)
        {
            _helper = helper;
            WorkspaceID = workspaceId;
            _dbContextFactory = new DbContextFactory(helper);
        }

        public int WorkspaceID { get; }

        public int GetEddsUserID() => _helper.GetAuthenticationManager().UserInfo.ArtifactID;

        public int GetWorkspaceUserID() => _helper.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID;

        public IRelativityObjectManagerService GetRelativityObjectManagerService()
        {
            return WorkspaceID > 0
                ? ServiceContextFactory.CreateRelativityObjectManagerService(_helper, WorkspaceID)
                : null;
        }

        public IRipDBContext GetDBContext(int workspaceID = -1) => _dbContextFactory.CreateWorkspaceDbContext(workspaceID);
    }
}
