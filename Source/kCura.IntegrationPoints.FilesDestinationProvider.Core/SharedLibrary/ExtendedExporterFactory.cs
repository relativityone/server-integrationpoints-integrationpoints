using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Windows.Process;
using kCura.WinEDDS;
using kCura.WinEDDS.Core.Export;
using kCura.WinEDDS.Core.IO;
using kCura.WinEDDS.Core.Model.Export;
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

		private ExtendedExporter Create(ExtendedExportFile exportFile, Controller processController, IServiceFactory serviceFactory,
			ILoadFileHeaderFormatterFactory loadFileFormatterFactory, bool nameTextAndNativesAfterBegBates, IFileNameProvider fileNameProvider)
		{
			return new ExtendedExporter(exportFile, processController, serviceFactory, loadFileFormatterFactory, new ExportConfig())
			{
				NameTextAndNativesAfterBegBates = nameTextAndNativesAfterBegBates,
				FileHelper = new LongPathFileHelper(),
				DirectoryHelper = new LongPathDirectoryHelper(),
				FileNameProvider = fileNameProvider
			};
		}

		public IExporter Create(ExportDataContext context)
		{
			var config = _configFactory.BuildFactoryConfig(context);

			var exporter = Create(context.ExportFile, config.Controller,
				config.ServiceFactory,
				config.LoadFileFormatterFactory, config.NameTextAndNativesAfterBegBates, config.FileNameProvider);

			return new StoppableExporter(exporter, config.Controller, config.JobStopManager);
		}
	}
}
