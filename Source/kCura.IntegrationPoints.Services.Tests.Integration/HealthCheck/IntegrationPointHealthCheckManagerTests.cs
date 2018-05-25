using kCura.IntegrationPoints.Services.Tests.Integration.Permissions;
using NUnit.Framework;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Services.Tests.Integration.HealthCheck
{
    public class IntegrationPointHealthCheckManagerTests : KeplerServicePermissionsTestsBase
    {
        [Test]
        public void ShouldBeHealthyByDefault()
        {
            IIntegrationPointHealthCheckManager client = Helper.CreateUserProxy<IIntegrationPointHealthCheckManager>(UserModel.EmailAddress);

            HealthCheckOperationResult result = client.RunHealthChecksAsync().Result;

            Assert.IsTrue(result.IsHealthy);
        }
    }
}