using Autofac;

namespace Relativity.Sync
{
	internal interface IContainerFactory
	{
		void RegisterSyncDependencies(ContainerBuilder containerBuilder);
	}
}