using System.Linq;
using System.Net;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication
{
	public class TokenCredentialProvider : ICredentialProvider
	{
		private const string _RELATIVITY_BEARER_USERNAME = "XxX_BearerTokenCredentials_XxX";

		public NetworkCredential Authenticate(CookieContainer cookieContainer)
		{
			string token = System.Security.Claims.ClaimsPrincipal.Current.Claims.Single(x => x.Type.Equals("access_token")).Value;
			return LoginHelper.LoginUsernamePassword(_RELATIVITY_BEARER_USERNAME, token, cookieContainer);
		}
	}
}
