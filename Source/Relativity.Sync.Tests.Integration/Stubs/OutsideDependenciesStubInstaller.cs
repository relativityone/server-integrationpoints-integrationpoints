using System;
using System.Collections.Generic;
using Autofac;
using Moq;
using Relativity.Telemetry.APM;
using Relativity.API;

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
			// Relativity.Telemetry.APM
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

			// Relativity.API
			Mock<IHelper> helperMock = new Mock<IHelper>();
			Mock<IDBContext> dbContextMock = new Mock<IDBContext>();
			helperMock.Setup(h => h.GetDBContext(It.IsAny<int>())).Returns(dbContextMock.Object);
			builder.RegisterInstance(helperMock.Object).As<IHelper>();
		}
	}
}
