using System.Net;
using kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi.WebApi
{
	public class FakeLoginHelperFacade : ILoginHelperFacade
	{
		public NetworkCredential LoginUsingAuthToken(string token, CookieContainer cookieContainer)
		{
			return new NetworkCredential();
		}
	}
}