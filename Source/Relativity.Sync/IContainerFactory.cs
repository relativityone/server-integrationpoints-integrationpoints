using Autofac;

namespace Relativity.Sync
{
	internal interface IContainerFactory
	{
		void RegisterSyncDependencies(ContainerBuilder containerBuilder, SyncJobParameters syncJobParameters, IRelativityServices relativityServices, SyncJobExecutionConfiguration configuration,
			ISyncLog logger);
	}
}