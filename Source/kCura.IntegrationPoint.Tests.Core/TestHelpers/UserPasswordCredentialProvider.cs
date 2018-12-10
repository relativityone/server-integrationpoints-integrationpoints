using System.Net;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class UserPasswordCredentialProvider : ICredentialProvider
	{
		public NetworkCredential Authenticate(CookieContainer cookieContainer)
		{
			return LoginHelper.LoginUsernamePassword(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, cookieContainer);
		}
	}
}
