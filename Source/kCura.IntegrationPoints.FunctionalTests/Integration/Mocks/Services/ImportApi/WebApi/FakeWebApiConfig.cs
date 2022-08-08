using kCura.IntegrationPoints.Config;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi.WebApi
{
    public class FakeWebApiConfig : IWebApiConfig
    {
        public string GetWebApiUrl { get; } = "https://fake.uri/fakepath";
    }
}