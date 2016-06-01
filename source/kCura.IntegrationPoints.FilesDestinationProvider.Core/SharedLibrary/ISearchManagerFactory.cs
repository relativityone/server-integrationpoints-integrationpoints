using System.Net;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public interface ISearchManagerFactory
    {
        ISearchManager Create(ICredentials credentials, CookieContainer cookieContainer);
    }
}