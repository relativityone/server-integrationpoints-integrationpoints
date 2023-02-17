using System.Net;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public interface ICaseManagerFactory
    {
        ICaseManager Create(ICredentials credentials, CookieContainer cookieContainer);
    }
}
