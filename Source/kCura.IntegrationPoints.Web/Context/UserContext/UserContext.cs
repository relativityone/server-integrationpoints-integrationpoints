using kCura.IntegrationPoints.Web.Infrastructure.Session;

namespace kCura.IntegrationPoints.Web.Context.UserContext
{
	public class UserContext : IUserContext
	{
		private readonly ISessionService _sessionService;

		public UserContext(ISessionService sessionService)
		{
			_sessionService = sessionService;
		}

		public int UserID => _sessionService.UserID;

		public int WorkspaceUserID => _sessionService.WorkspaceUserID;
	}
}