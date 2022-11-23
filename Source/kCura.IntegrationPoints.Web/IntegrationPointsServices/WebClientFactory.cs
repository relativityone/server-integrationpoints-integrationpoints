using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Data.DbContext;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.IntegrationPointsServices
{
    public class WebClientFactory
    {
        private readonly IHelper _helper;
        private readonly IWorkspaceContext _workspaceIdProvider;

        public WebClientFactory(IHelper helper, IWorkspaceContext workspaceIdProvider)
        {
            _helper = helper;
            _workspaceIdProvider = workspaceIdProvider;
        }

        public IWorkspaceDBContext CreateWorkspaceDbContext()
        {
            IDbContextFactory dbContextFactory = new DbContextFactory(_helper);
            int workspaceId = _workspaceIdProvider.GetWorkspaceID();
            return dbContextFactory.CreateWorkspaceDbContext(workspaceId);
        }

        public IDBContext CreateDbContext()
        {
            int workspaceId = _workspaceIdProvider.GetWorkspaceID();
            return _helper.GetDBContext(workspaceId);
        }
    }
}
