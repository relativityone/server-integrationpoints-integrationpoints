using kCura.IntegrationPoints.Web.Infrastructure.Session;

namespace kCura.IntegrationPoints.Web.Context.UserContext
{
    public class SessionUserContextService : IUserContext
    {
        private readonly ISessionService _sessionService;
        private readonly IUserContext _nextUserContextService;

        public SessionUserContextService(
            ISessionService sessionService,
            IUserContext nextUserContextService)
        {
            _sessionService = sessionService;
            _nextUserContextService = nextUserContextService;
        }

        public int GetUserID() =>
            _sessionService.UserID ?? _nextUserContextService.GetUserID();

        public int GetWorkspaceUserID() =>
            _sessionService.WorkspaceUserID ?? _nextUserContextService.GetWorkspaceUserID();
    }
}
