using System.Net;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    internal class CaseManagerFactory : ICaseManagerFactory
    {
        public ICaseManager Create(ICredentials credentials, CookieContainer cookieContainer)
        {
            return new CaseManager(credentials, cookieContainer);
        }
    }
}
