using System.Net;
using kCura.WinEDDS.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    internal class SearchManagerWrapperFactory : ISearchManagerFactory
    {
        public ISearchManager Create(ICredentials credentials, CookieContainer cookieContainer)
        {
            return new SearchManagerWrapper(new SearchManager(credentials, cookieContainer));
        }
    }
}