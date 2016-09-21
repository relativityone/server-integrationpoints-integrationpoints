using System.Net;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	internal class ProductionManagerFactory : IManagerFactory<IProductionManager>
	{
		public IProductionManager Create(ICredentials credentials, CookieContainer cookieContainer)
		{
			return new ProductionManager(credentials, cookieContainer);
		}
	}
}