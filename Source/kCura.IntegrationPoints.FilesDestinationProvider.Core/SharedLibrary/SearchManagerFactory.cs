using System;
using System.Net;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Toggles;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class SearchManagerFactory : IServiceManagerFactory<ISearchManager>
	{
		public ISearchManager Create(ICredentials credentials, CookieContainer cookieContainer, string webServiceUrl = null)
		{
			if (ToggleProvider.Current.IsEnabled<EnableKeplerizedImportAPIToggle>())
			{
				throw new InvalidOperationException(
					"Keplerized Import API is on, SearchManagerFactory.Create should not be called");
			}

			return new SearchManager(credentials, cookieContainer);
		}
	}
}