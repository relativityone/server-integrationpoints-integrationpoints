using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Web.Infrastructure.Session;

namespace kCura.IntegrationPoints.Web.Context.WorkspaceContext
{
    internal class SessionWorkspaceContextService : IWorkspaceContext
    {
        private readonly ISessionService _sessionService;
        private readonly IWorkspaceContext _nextWorkspaceContextService;

        public SessionWorkspaceContextService(
            ISessionService sessionService,
            IWorkspaceContext nextWorkspaceContextService)
        {
            _sessionService = sessionService;
            _nextWorkspaceContextService = nextWorkspaceContextService;
        }

        public int GetWorkspaceID() =>
            _sessionService.WorkspaceID ?? _nextWorkspaceContextService.GetWorkspaceID();
    }
}
