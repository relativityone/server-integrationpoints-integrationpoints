using System.Net;
using kCura.WinEDDS.Api;
using Relativity.DataExchange;
using Relativity.DataExchange.Service;

namespace kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade
{
	internal class LoginHelperFacade : ILoginHelperFacade
	{
		private const string _RELATIVITY_BEARER_USERNAME = "XxX_BearerTokenCredentials_XxX";
		
		public NetworkCredential LoginUsingAuthToken(string token, CookieContainer cookieContainer)
		{
			return LoginHelper.LoginUsernamePassword(_RELATIVITY_BEARER_USERNAME, token, cookieContainer, new RunningContext
			{
				ExecutionSource = ExecutionSource.RIP
			});
		}
	}
}