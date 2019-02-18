using Relativity.API;

namespace kCura.IntegrationPoints.Web.Infrastructure.Session
{
	public class SessionService : ISessionService
	{
		private readonly ICPHelper _connectionHelper;

		public SessionService(ICPHelper connectionHelper)
		{
			_connectionHelper = connectionHelper;
		}

		public int WorkspaceID => _connectionHelper.GetActiveCaseID(); // TODO maybe this service should be internal(infrastructure) and another service for usercontext should be created to use

		public int UserID => _connectionHelper.GetAuthenticationManager().UserInfo.ArtifactID;

		public int WorkspaceUserID => _connectionHelper.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID;
	}
}