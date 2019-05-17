using System.Linq;
using System.Reflection;
using Autofac;

namespace Relativity.Sync.Transfer
{
	internal sealed class TransferInstaller : IInstaller
	{
		public void Install(ContainerBuilder builder)
		{
			builder.RegisterType<SourceWorkspaceDataTableBuilderFactory>().As<ISourceWorkspaceDataTableBuilderFactory>();
			builder.RegisterType<RelativityExportBatcher>().As<IRelativityExportBatcher>();
			builder.RegisterType<NativeFileRepository>().As<INativeFileRepository>();
			builder.RegisterType<FieldManager>().As<IFieldManager>();
			builder.RegisterType<FolderPathRetriever>().As<IFolderPathRetriever>();
			builder.RegisterTypes(Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => !t.IsAbstract && t.IsAssignableTo<ISpecialFieldBuilder>())
				.ToArray()).As<ISpecialFieldBuilder>();
		}
	}
}
