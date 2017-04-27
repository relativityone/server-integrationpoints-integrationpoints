using System.Net;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IServiceManagerFactory<out TManager>
	{
		TManager Create(ICredentials credentials, CookieContainer cookieContainer, string webServiceUrl = null);
	}
}
