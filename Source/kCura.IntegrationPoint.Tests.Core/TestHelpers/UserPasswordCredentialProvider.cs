using System.Net;
using kCura.IntegrationPoints.Core.Authentication.WebApi;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class UserPasswordCredentialProvider : IWebApiLoginService
	{
		public NetworkCredential Authenticate(CookieContainer cookieContainer)
		{
			return LoginHelper.LoginUsernamePassword(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, cookieContainer);
		}
	}
}
