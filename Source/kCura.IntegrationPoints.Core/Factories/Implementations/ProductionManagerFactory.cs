using System;
using System.Net;
using kCura.IntegrationPoints.Data.Toggles;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ProductionManagerFactory : IServiceManagerFactory<IProductionManager>
	{
		public IProductionManager Create(ICredentials credentials, CookieContainer cookieContainer, string webServiceUrl)
		{
            if (ToggleProvider.Current.IsEnabled<EnableKeplerizedImportAPIToggle>())
            {
                throw new InvalidOperationException(
					"Keplerized Import API is on, ProductionManagerFactory.Create should not be called");
            }
			return new ProductionManager(credentials, cookieContainer, webServiceUrl);
		}
	}
}