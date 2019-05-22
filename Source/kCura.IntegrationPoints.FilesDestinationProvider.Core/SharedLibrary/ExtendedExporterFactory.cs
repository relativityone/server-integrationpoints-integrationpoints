using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class ExtendedExporterFactory : IExtendedExporterFactory
	{
		private readonly IFactoryConfigBuilder _configFactory;


		public ExtendedExporterFactory(IFactoryConfigBuilder factoryConfigBuilder)
		{
			_configFactory = factoryConfigBuilder;
		}

		public ExtendedExporter Create(ExtendedExportFile exportFile, global::Relativity.DataExchange.Process.ProcessContext context, ILoadFileHeaderFormatterFactory loadFileFormatterFactory)
		{
			return new ExtendedExporter(exportFile, context, loadFileFormatterFactory);
		}

		private ExtendedExporter Create(ExtendedExportFile exportFile, ExporterFactoryConfig config)
		{
			return new ExtendedExporter(exportFile, config.Controller, config.ServiceFactory, config.LoadFileFormatterFactory, config.ExportConfig)
			{
				NameTextAndNativesAfterBegBates = config.NameTextAndNativesAfterBegBates,
				FileHelper = new global::Relativity.DataExchange.Io.LongPathFileHelper(),
				DirectoryHelper = new global::Relativity.DataExchange.Io.LongPathDirectoryHelper(),
				FileNameProvider = config.FileNameProvider
			};
		}

		public IExporter Create(ExportDataContext context, IServiceFactory serviceFactory)
		{
			ExporterFactoryConfig config = _configFactory.BuildFactoryConfig(context, serviceFactory);

			ExtendedExporter exporter = Create(context.ExportFile, config);

			kCura.WinEDDS.Container.ContainerFactoryProvider.ContainerFactory = new global::Relativity.DataExchange.Export.VolumeManagerV2.Container.ContainerFactory();

            return new StoppableExporter(exporter, config.Controller, config.JobStopManager);
		}
	}
}
