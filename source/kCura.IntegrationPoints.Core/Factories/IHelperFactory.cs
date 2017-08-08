using System.Net;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories
{
	public class WsInstanceInfo
	{
		public NetworkCredential NetworkCredential;
		public string WebServiceUrl;
	}

	public interface IHelperFactory
	{
		IHelper CreateTargetHelper(IHelper sourceInstanceHelper, int? federatedInstanceArtifactId, string credentials);

		WsInstanceInfo GetNetworkCredential(IHelper sourceInstanceHelper, int? federatedInstanceArtifactId,
			string credentials, CookieContainer cookieContainer);
	}
}