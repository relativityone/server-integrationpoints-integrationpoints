using System;
using System.Net;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Data.Toggles;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class SearchManagerFactory : IServiceManagerFactory<ISearchManager>
	{
		public ISearchManager Create(ICredentials credentials, CookieContainer cookieContainer, string webServiceUrl = null)
		{
			return new SearchManager(credentials, cookieContainer);
		}
	}
}