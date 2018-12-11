namespace kCura.IntegrationPoints.Web.Services
{
	public interface ISessionService
	{
		int WorkspaceID { get; }
		int UserID { get; }
		int WorkspaceUserID { get; }
	}
}
