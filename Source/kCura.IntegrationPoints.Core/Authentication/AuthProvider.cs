using System.Net;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoints.Core.Authentication
{
	internal class AuthProvider : IAuthProvider
	{
		private const string _RELATIVITY_BEARER_USERNAME = "XxX_BearerTokenCredentials_XxX";

		public NetworkCredential LoginUsingWinAuth(CookieContainer cookieContainer)
		{
			return LoginHelper.LoginWindowsAuth(cookieContainer);
		}

		public NetworkCredential LoginUsingUserNameAndPassword(string username, string password, CookieContainer cookieContainer)
		{
			return LoginHelper.LoginUsernamePassword(username, password, cookieContainer);
		}

		public NetworkCredential LoginUsingAuthToken(string token, CookieContainer cookieContainer)
		{
			return LoginHelper.LoginUsernamePassword(_RELATIVITY_BEARER_USERNAME, token, cookieContainer);
		}
	}
}