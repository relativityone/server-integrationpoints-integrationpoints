using kCura.IntegrationPoints.Services.Tests.Integration.Permissions;
using NUnit.Framework;
using Relativity.Telemetry.APM;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Services.Tests.Integration.HealthCheck
{
	public class IntegrationPointHealthCheckManagerTests : KeplerServicePermissionsTestsBase
	{
		[IdentifiedTest("bc05ee7f-a723-4385-9778-557ca2d8a7a4")]
		public void ShouldBeHealthyByDefault()
		{
			IIntegrationPointHealthCheckManager client = Helper.CreateUserProxy<IIntegrationPointHealthCheckManager>(UserModel.EmailAddress);

			HealthCheckOperationResult result = client.RunHealthChecksAsync().Result;

			Assert.IsTrue(result.IsHealthy);
		}
	}
}