using System.Net;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoints.Core.Authentication.AuthProvider
{
	internal class AuthProvider : IAuthProvider
	{
		private const string _RELATIVITY_BEARER_USERNAME = "XxX_BearerTokenCredentials_XxX";
		
		public NetworkCredential LoginUsingAuthToken(string token, CookieContainer cookieContainer)
		{
			return LoginHelper.LoginUsernamePassword(_RELATIVITY_BEARER_USERNAME, token, cookieContainer);
		}
	}
}