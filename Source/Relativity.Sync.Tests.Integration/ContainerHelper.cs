using Autofac;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Integration
{
	internal static class ContainerHelper
	{
		public static ContainerBuilder CreateInitializedContainerBuilder()
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();
			ContainerFactory containerFactory = new ContainerFactory(new SyncJobParameters(1, 1), new SyncConfiguration(), new EmptyLogger());
			containerFactory.RegisterSyncDependencies(containerBuilder);
			return containerBuilder;
		}

		public static IContainer CreateInitializedContainer()
		{
			ContainerBuilder containerBuilder = CreateInitializedContainerBuilder();
			return containerBuilder.Build();
		}
	}
}