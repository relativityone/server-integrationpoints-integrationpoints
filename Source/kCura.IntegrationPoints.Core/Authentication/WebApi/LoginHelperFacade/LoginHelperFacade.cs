using System.Net;
using kCura.WinEDDS.Api;
using Relativity.DataExchange;
using Relativity.DataExchange.Service;

namespace kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade
{
	internal class LoginHelperFacade : ILoginHelperFacade
	{
		
		
		public NetworkCredential LoginUsingAuthToken(string token, CookieContainer cookieContainer)
		{
			//[REL-838809]: Resolve correlationIdFunc
			return LoginHelper.LoginUsernamePassword(AuthConstants._RELATIVITY_BEARER_USERNAME, token, cookieContainer, new RunningContext
			{
				ExecutionSource = ExecutionSource.RIP
			},() => string.Empty);
		}
	}
}