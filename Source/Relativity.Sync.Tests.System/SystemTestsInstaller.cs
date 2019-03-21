using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Moq;
using Relativity.Telemetry.APM;
using Relativity.API;
using Relativity.Sync.Telemetry;
using Relativity.Services.InstanceSetting;
using Relativity.Sync.Tests.System.Stubs;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	/// <summary>
	///     Installer for mocks/stubs around external dependencies that we can't/won't
	///     reference during integration tests.
	/// </summary>
	internal sealed class SystemTestsInstaller : IInstaller
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
			Mock<IAPILog> apiLogMock = new Mock<IAPILog>();
			builder.RegisterType<ServicesManagerStub>().As<IServicesMgr>();
			builder.RegisterInstance(apiLogMock.Object).As<IAPILog>();
			builder.RegisterType<ProvideServiceUrisStub>().As<IProvideServiceUris>();
		}
	}
}
