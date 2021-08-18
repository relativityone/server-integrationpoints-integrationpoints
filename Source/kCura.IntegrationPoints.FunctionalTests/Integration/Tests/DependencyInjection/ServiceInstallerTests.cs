using Castle.Core;
using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Integration.Assertions;
using Relativity.Telemetry.APM;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.DependencyInjection
{
	public class ServiceInstallerTests : TestsBase
	{
		[IdentifiedTest("D9849E33-A9A5-4AA6-98E0-80ECECC09C5F")]
		public void IAPM_ShouldBeRegisteredAsSingleton()
		{
			// Act & Assert
			Container.Should()
				.HaveRegisteredSingleComponent<IAPM>()
				.Which.Should().BeRegisteredWithLifestyle(LifestyleType.Singleton);
		}
	}
}
