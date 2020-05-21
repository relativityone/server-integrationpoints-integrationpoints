using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.Tests.Integration.Permissions;
using Relativity.Telemetry.APM;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.HealthCheck
{
	[Feature.DataTransfer.IntegrationPoints]
	[NotWorkingOnTrident]
	public class IntegrationPointHealthCheckManagerTests : KeplerServicePermissionsTestsBase
	{
		[IdentifiedTest("bc05ee7f-a723-4385-9778-557ca2d8a7a4")]
		public void ShouldBeHealthyByDefault()
		{
			IIntegrationPointHealthCheckManager client = Helper.CreateProxy<IIntegrationPointHealthCheckManager>(UserModel.EmailAddress);

			HealthCheckOperationResult result = client.RunHealthChecksAsync().Result;

			Assert.IsTrue(result.IsHealthy);
		}
	}
}