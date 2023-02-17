using System.Web;
using kCura.IntegrationPoints.Common.Context;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceContext
{
    internal class RequestContextWorkspaceContextService : IWorkspaceContext
    {
        private const string _WORKSPACE_ID_KEY = "workspaceID";
        private readonly HttpRequestBase _httpRequest;
        private readonly IWorkspaceContext _nextWorkspaceContextService;

        public RequestContextWorkspaceContextService(
            HttpRequestBase httpRequest,
            IWorkspaceContext nextWorkspaceContextService)
        {
            _httpRequest = httpRequest;
            _nextWorkspaceContextService = nextWorkspaceContextService;
        }

        public int GetWorkspaceID()
        {
            var workspaceIdRouteData = _httpRequest.RequestContext.RouteData.Values[_WORKSPACE_ID_KEY] as string;
            bool isWorkspaceIdValidNumber = int.TryParse(workspaceIdRouteData, out int workspaceId);

            return isWorkspaceIdValidNumber
                ? workspaceId
                : _nextWorkspaceContextService.GetWorkspaceID();
        }
    }
}
