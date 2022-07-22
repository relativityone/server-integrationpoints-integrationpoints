using Autofac;
using Relativity.AntiMalware.SDK;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Extensions;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Transfer
{
    internal sealed class TransferInstaller : IInstaller
    {
        public void Install(ContainerBuilder builder)
        {
            builder.RegisterType<ObjectFieldTypeRepository>().As<IObjectFieldTypeRepository>();
            builder.RegisterType<RelativityExportBatcher>().As<IRelativityExportBatcher>();
            builder.RegisterType<NativeFileRepository>().As<INativeFileRepository>();
            builder.RegisterType<ImageFileRepository>().As<IImageFileRepository>();
            builder.RegisterType<FileStatisticsCalculator>().As<IFileStatisticsCalculator>();
            builder.RegisterType<FieldManager>().As<IFieldManager>();
            builder.RegisterType<ExportDataSanitizer>().As<IExportDataSanitizer>();
            builder.RegisterType<FolderPathRetriever>().As<IFolderPathRetriever>();
            builder.RegisterType<ChoiceTreeToStringConverter>().As<IChoiceTreeToStringConverter>();
            builder.RegisterType<ChoiceCache>().As<IChoiceCache>();
            builder.RegisterType<SourceWorkspaceDataReaderFactory>().As<ISourceWorkspaceDataReaderFactory>();
            builder.RegisterType<RelativityExportBatcherFactory>().As<IRelativityExportBatcherFactory>();
            builder.RegisterType<ImportStreamBuilder>().As<IImportStreamBuilder>();
            builder.RegisterType<StreamRetryPolicyFactory>().As<IStreamRetryPolicyFactory>();
            builder.RegisterTypesInExecutingAssembly<INativeSpecialFieldBuilder>();
            builder.RegisterTypesInExecutingAssembly<INativeInfoFieldsBuilder>();
            builder.RegisterTypesInExecutingAssembly<IImageSpecialFieldBuilder>();
            builder.RegisterTypesInExecutingAssembly<IExportFieldSanitizer>();
            builder.RegisterType<RetriableLongTextStreamBuilderFactory>().As<IRetriableStreamBuilderFactory>();
            builder.RegisterType<InstanceSettings>().As<IInstanceSettings>();
            builder.RegisterType<SnapshotQueryRequestProvider>().As<ISnapshotQueryRequestProvider>();

            builder.Register(c => c.Resolve<IHelper>().GetAntiMalwareEventHelper()).As<IAntiMalwareEventHelper>();
            builder.RegisterType<AntiMalwareHandler>().As<IAntiMalwareHandler>();
            builder.RegisterType<AntiMalwareConfiguration>().As<IAntiMalwareConfiguration>();
        }
    }
}
