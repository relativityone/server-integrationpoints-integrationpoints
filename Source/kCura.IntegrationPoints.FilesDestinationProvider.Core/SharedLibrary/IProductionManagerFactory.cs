using System.Net;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    internal interface IProductionManagerFactory
    {
        IProductionManager Create(ICredentials credentials, CookieContainer cookieContainer);
    }
}
