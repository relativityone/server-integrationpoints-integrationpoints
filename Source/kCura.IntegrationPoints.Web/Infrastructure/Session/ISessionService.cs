namespace kCura.IntegrationPoints.Web.Infrastructure.Session
{
    public interface ISessionService
    {
        int? WorkspaceID { get; }
        int? UserID { get; }
        int? WorkspaceUserID { get; }
    }
}
