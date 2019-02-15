using Relativity.API;

namespace kCura.IntegrationPoints.Web.Services
{
	public class SessionService : ISessionService
	{
		private readonly ICPHelper _connectionHelper;

		public SessionService(ICPHelper connectionHelper)
		{
			_connectionHelper = connectionHelper;
		}

		public int WorkspaceID => _connectionHelper.GetActiveCaseID();

		public int UserID => _connectionHelper.GetAuthenticationManager().UserInfo.ArtifactID;

		public int WorkspaceUserID => _connectionHelper.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID;
	}
}