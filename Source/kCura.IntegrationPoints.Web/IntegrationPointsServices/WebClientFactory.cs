using kCura.IntegrationPoints.Common.Context;
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

        public IDBContext CreateDbContext()
        {
            int workspaceId = _workspaceIdProvider.GetWorkspaceID();
            return _helper.GetDBContext(workspaceId);
        }
    }
}
