using Autofac;

namespace Relativity.Sync
{
	internal interface IContainerFactory
	{
		void RegisterSyncDependencies(ContainerBuilder containerBuilder, SyncJobParameters syncJobParameters, SyncConfiguration configuration, ISyncLog logger);
	}
}