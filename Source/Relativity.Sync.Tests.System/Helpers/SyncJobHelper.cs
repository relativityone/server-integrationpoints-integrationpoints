using Autofac;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal static class SyncJobHelper
	{
		public static ISyncJob CreateWithMockedContainerExceptProvidedType<TStepConfiguration>(ConfigurationStub configuration)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();

			ContainerFactory factory = new ContainerFactory();
			SyncJobParameters syncParameters = new SyncJobParameters(configuration.JobArtifactId, configuration.SourceWorkspaceArtifactId);
			factory.RegisterSyncDependencies(containerBuilder, syncParameters, new SyncJobExecutionConfiguration(), new EmptyLogger());

			new SystemTestsInstaller().Install(containerBuilder);

			IntegrationTestsContainerBuilder.RegisterExternalDependenciesAsMocks(containerBuilder);
			IntegrationTestsContainerBuilder.MockStepsExcept<TStepConfiguration>(containerBuilder);

			containerBuilder.RegisterInstance(configuration).AsImplementedInterfaces();

			return containerBuilder.Build().Resolve<ISyncJob>();
		}
	}
}
