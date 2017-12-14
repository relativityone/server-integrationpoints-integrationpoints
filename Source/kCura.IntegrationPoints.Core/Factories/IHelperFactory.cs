using System.Net;

namespace kCura.IntegrationPoints.Core.Factories
{
	public class WsInstanceInfo
	{
		public NetworkCredential NetworkCredential;
		public string WebServiceUrl;
	}

	public interface IHelperFactory
	{
		WsInstanceInfo GetNetworkCredential(CookieContainer cookieContainer);
	}
}