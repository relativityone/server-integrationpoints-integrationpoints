using System.Threading;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity.Logging;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class ExtendedExporterFactory : IExtendedExporterFactory
	{
		private readonly IFactoryConfigBuilder _configFactory;
		private readonly ILog _logger;
		
		public ExtendedExporterFactory(IFactoryConfigBuilder factoryConfigBuilder, ILog logger)
		{
			_configFactory = factoryConfigBuilder;
			_logger = logger;
		}

		public IExporter Create(ExportDataContext context, IServiceFactory serviceFactory)
		{
			ExporterFactoryConfig config = _configFactory.BuildFactoryConfig(context, serviceFactory);
			ExtendedExporter exporter = Create(context.ExportFile, config);
			kCura.WinEDDS.Container.ContainerFactoryProvider.ContainerFactory = new global::Relativity.DataExchange.Export.VolumeManagerV2.Container.ContainerFactory(_logger);
            return new StoppableExporter(exporter, config.Controller, config.JobStopManager);
		}

		private ExtendedExporter Create(ExtendedExportFile exportFile, ExporterFactoryConfig config)
		{
			return new ExtendedExporter(exportFile, config.Controller, config.ServiceFactory, config.LoadFileFormatterFactory, config.ExportConfig, _logger, CancellationToken.None)
			{
				NameTextAndNativesAfterBegBates = config.NameTextAndNativesAfterBegBates,
				FileHelper = new LongPathFileHelper(),
				DirectoryHelper = new LongPathDirectoryHelper(),
				FileNameProvider = config.FileNameProvider
			};
		}
	}
}
