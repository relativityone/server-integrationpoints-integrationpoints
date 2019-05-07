using System;
using System.Collections.Generic;
using Autofac;
using Moq;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Stubs;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal static class SyncJobHelper
	{
		public static ISyncJob CreateWithMockedContainerExceptProvidedType<TStepConfiguration>(ConfigurationStub configuration)
		{
			return Create(configuration, IntegrationTestsContainerBuilder.MockStepsExcept<TStepConfiguration>);
		}

		public static ISyncJob CreateWithMockedAllSteps(ConfigurationStub configuration)
		{
			return Create(configuration, IntegrationTestsContainerBuilder.MockAllSteps);
		}

		private static ISyncJob Create(ConfigurationStub configuration, Action<ContainerBuilder> mockSteps)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();

			ContainerFactory factory = new ContainerFactory();
			SyncJobParameters syncParameters = new SyncJobParameters(configuration.JobArtifactId, configuration.SourceWorkspaceArtifactId);
			
			IAPM apm = CreateMockedAPM();
			RelativityServices relativityServices = new RelativityServices(apm, new ServicesManagerStub(), AppSettings.RelativityUrl);

			factory.RegisterSyncDependencies(containerBuilder, syncParameters, relativityServices, new SyncJobExecutionConfiguration(), new ConsoleLogger());
			
			mockSteps.Invoke(containerBuilder);

			containerBuilder.RegisterInstance(configuration).AsImplementedInterfaces();

			return containerBuilder.Build().Resolve<ISyncJob>();
		}

		private static IAPM CreateMockedAPM()
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
			return apmMock.Object;
		}
	}
}