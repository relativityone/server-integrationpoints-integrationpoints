using System.Net;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public interface ISearchManagerFactory
    {
        ISearchManager Create(ICredentials credentials, CookieContainer cookieContainer);
    }
}