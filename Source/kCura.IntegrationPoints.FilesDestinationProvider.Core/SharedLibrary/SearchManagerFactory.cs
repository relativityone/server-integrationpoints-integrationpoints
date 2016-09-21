using System;
using System.Net;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    internal class SearchManagerFactory : IManagerFactory<ISearchManager>
    {
        public ISearchManager Create(ICredentials credentials, CookieContainer cookieContainer)
        {
            return new SearchManager(credentials, cookieContainer);
        }
	}
}