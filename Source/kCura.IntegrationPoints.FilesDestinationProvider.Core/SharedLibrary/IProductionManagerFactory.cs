using System.Net;
using kCura.WinEDDS.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	internal interface IProductionManagerFactory
	{
		ProductionManager Create(ICredentials credentials, CookieContainer cookieContainer);
	}
}