using Moq;
using Relativity.Services.Environmental;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Kepler
{
    public class PingServiceStub : KeplerStubBase<IPingService>
    {
        public void SetupPingService()
        {
            Mock.Setup(x => x.Ping()).ReturnsAsync("OK");
        }
    }
}
