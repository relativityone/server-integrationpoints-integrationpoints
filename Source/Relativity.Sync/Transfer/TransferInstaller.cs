using Autofac;

namespace Relativity.Sync.Transfer
{
	internal sealed class TransferInstaller : IInstaller
	{
		public void Install(ContainerBuilder builder)
		{
			builder.RegisterType<BatchDataTableBuilderFactory>().As<IBatchDataTableBuilderFactory>();
			builder.RegisterType<NativeFileRepository>().As<INativeFileRepository>();
			builder.RegisterType<RelativityExportBatcher>().As<IRelativityExportBatcher>();
			builder.RegisterType<FolderPathRetriever>().As<IFolderPathRetriever>();
		}
	}
}
