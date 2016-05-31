using System.Net;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public interface ICaseManagerFactory
    {
        ICaseManager Create(ICredentials credentials, CookieContainer cookieContainer);
    }
}