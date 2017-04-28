using System.Net;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ProductionManagerFactory : IServiceManagerFactory<IProductionManager>
	{
		public IProductionManager Create(ICredentials credentials, CookieContainer cookieContainer, string webServiceUrl)
		{
			return new ProductionManager(credentials, cookieContainer, webServiceUrl);
		}
	}
}