using System.Linq;
using System.Reflection;
using Autofac;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Transfer
{
	internal sealed class TransferInstaller : IInstaller
	{
		public void Install(ContainerBuilder builder)
		{
			builder.RegisterType<DocumentFieldRepository>().As<IDocumentFieldRepository>();
			builder.RegisterType<BatchDataReaderBuilder>().As<IBatchDataReaderBuilder>();
			builder.RegisterType<RelativityExportBatcher>().As<IRelativityExportBatcher>();
			builder.RegisterType<SearchManagerFactory>().As<ISearchManagerFactory>().SingleInstance();
			builder.RegisterType<NativeFileRepository>().As<INativeFileRepository>();
			builder.RegisterType<ImageFileRepository>().As<IImageFileRepository>();
			builder.RegisterType<FieldManager>().As<IFieldManager>();
			builder.RegisterType<ExportDataSanitizer>().As<IExportDataSanitizer>();
			builder.RegisterType<FolderPathRetriever>().As<IFolderPathRetriever>();
			builder.RegisterType<ChoiceTreeToStringConverter>().As<IChoiceTreeToStringConverter>();
			builder.RegisterType<ChoiceCache>().As<IChoiceCache>();
			builder.RegisterType<SourceWorkspaceDataReaderFactory>().As<ISourceWorkspaceDataReaderFactory>();
			builder.RegisterType<RelativityExportBatcherFactory>().As<IRelativityExportBatcherFactory>();
			builder.RegisterType<ImportStreamBuilder>().As<IImportStreamBuilder>();
			builder.RegisterType<StreamRetryPolicyFactory>().As<IStreamRetryPolicyFactory>();
			builder.RegisterTypes(Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => !t.IsAbstract && t.IsAssignableTo<ISpecialFieldBuilder>())
				.ToArray()).As<ISpecialFieldBuilder>();
			builder.RegisterTypes(Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => !t.IsAbstract && t.IsAssignableTo<IExportFieldSanitizer>())
				.ToArray()).As<IExportFieldSanitizer>();
			builder.RegisterType<RetriableLongTextStreamBuilderFactory>().As<IRetriableStreamBuilderFactory>();
			builder.RegisterType<InstanceSettings>().As<IInstanceSettings>();
		}
	}
}
