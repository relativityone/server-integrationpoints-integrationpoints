using System.Net;
using kCura.WinEDDS.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	internal class ProductionManagerFactory : IProductionManagerFactory
	{
		public ProductionManager Create(ICredentials credentials, CookieContainer cookieContainer)
		{
			return new ProductionManager(credentials, cookieContainer);
		}
	}
}