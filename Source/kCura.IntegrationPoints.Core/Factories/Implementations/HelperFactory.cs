
using System.Linq;
using System.Net;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class HelperFactory : IHelperFactory
	{
		private const string _RELATIVITY_BEARER_USERNAME = "XxX_BearerTokenCredentials_XxX";

		public WsInstanceInfo GetNetworkCredential(CookieContainer cookieContainer)
		{
			var token = System.Security.Claims.ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;
			return new WsInstanceInfo
			{
				NetworkCredential =
					LoginHelper.LoginUsernamePassword(_RELATIVITY_BEARER_USERNAME, token, cookieContainer),
				WebServiceUrl = WinEDDS.Config.WebServiceURL
			};
		}
	}
}