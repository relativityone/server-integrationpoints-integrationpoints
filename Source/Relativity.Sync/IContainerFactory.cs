using Autofac;
using Relativity.API;

namespace Relativity.Sync
{
	internal interface IContainerFactory
	{
		void RegisterSyncDependencies(ContainerBuilder containerBuilder, SyncJobParameters syncJobParameters, IRelativityServices relativityServices, SyncJobExecutionConfiguration configuration,
			IAPILog logger);
	}
}