using kCura.IntegrationPoints.Domain.Authentication;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi.WebApi
{
	public class FakeAuthTokenGenerator : IAuthTokenGenerator
	{
		public string GetAuthToken()
		{
			return "FakeAuthToken";
		}
	}
}