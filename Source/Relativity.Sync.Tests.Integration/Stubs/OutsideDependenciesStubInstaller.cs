using System;
using System.Collections.Generic;
using Autofac;
using Moq;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	/// <summary>
	///     Installer for mocks/stubs around external dependencies that we can't/won't
	///     reference during integration tests.
	/// </summary>
	internal sealed class OutsideDependenciesStubInstaller : IInstaller
	{
		public void Install(ContainerBuilder builder)
		{
			Mock<IAPM> apmMock = new Mock<IAPM>();
			Mock<ICounterMeasure> counterMock = new Mock<ICounterMeasure>();
			apmMock.Setup(a => a.CountOperation(It.IsAny<string>(),
				It.IsAny<Guid>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<bool>(),
				It.IsAny<int?>(),
				It.IsAny<Dictionary<string, object>>(),
				It.IsAny<IEnumerable<ISink>>())
			).Returns(counterMock.Object);

			builder.RegisterInstance(apmMock.Object).As<IAPM>();
		}
	}
}
