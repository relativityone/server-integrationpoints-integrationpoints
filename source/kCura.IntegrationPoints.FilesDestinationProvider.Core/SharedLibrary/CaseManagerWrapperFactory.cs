using System.Net;
using kCura.WinEDDS.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    internal class CaseManagerWrapperFactory : ICaseManagerFactory
    {
        public ICaseManager Create(ICredentials credentials, CookieContainer cookieContainer)
        {
            return new CaseManagerWrapper(new CaseManager(credentials, cookieContainer));
        }
    }
}