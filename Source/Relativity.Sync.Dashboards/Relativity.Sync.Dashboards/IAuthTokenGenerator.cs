namespace Relativity.Sync.Dashboards
{
    public interface IAuthTokenGenerator
    {
        string GetAuthToken(string userName, string password);
    }
}