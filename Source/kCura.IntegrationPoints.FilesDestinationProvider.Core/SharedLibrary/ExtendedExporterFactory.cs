using kCura.Windows.Process;
using kCura.WinEDDS;
using kCura.WinEDDS.Core.Export.VolumeManagerV2.Container;
using kCura.WinEDDS.Core.IO;
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

		public ExtendedExporter Create(ExtendedExportFile exportFile, Controller processController, ILoadFileHeaderFormatterFactory loadFileFormatterFactory)
		{
			return new ExtendedExporter(exportFile, processController, loadFileFormatterFactory);
		}

		private ExtendedExporter Create(ExtendedExportFile exportFile, ExporterFactoryConfig config)
		{
			return new ExtendedExporter(exportFile, config.Controller, config.ServiceFactory, config.LoadFileFormatterFactory, config.ExportConfig)
			{
				NameTextAndNativesAfterBegBates = config.NameTextAndNativesAfterBegBates,
				FileHelper = new LongPathFileHelper(),
				DirectoryHelper = new LongPathDirectoryHelper(),
				FileNameProvider = config.FileNameProvider
			};
		}

		public IExporter Create(ExportDataContext context, IServiceFactory serviceFactory)
		{
			ExporterFactoryConfig config = _configFactory.BuildFactoryConfig(context, serviceFactory);

			ExtendedExporter exporter = Create(context.ExportFile, config);

			kCura.WinEDDS.Container.ContainerFactoryProvider.ContainerFactory = new ContainerFactory();

			return new StoppableExporter(exporter, config.Controller, config.JobStopManager);
		}
	}
}
