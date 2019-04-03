using Autofac;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Integration
{
	internal static class ContainerHelper
	{
		public static ContainerBuilder CreateInitializedContainerBuilder()
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();
			ContainerFactory containerFactory = new ContainerFactory();
			containerFactory.RegisterSyncDependencies(containerBuilder, new SyncJobParameters(1, 1), new SyncJobExecutionConfiguration(), new EmptyLogger());
			return containerBuilder;
		}
	}
}