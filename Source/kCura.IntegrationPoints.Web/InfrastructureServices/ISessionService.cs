namespace kCura.IntegrationPoints.Web.InfrastructureServices
{
	public interface ISessionService
	{
		int WorkspaceID { get; }
		int UserID { get; }
		int WorkspaceUserID { get; }
	}
}
