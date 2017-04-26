using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Extensions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.Windows.Process;
using kCura.WinEDDS;
using kCura.WinEDDS.Core.Export;
using kCura.WinEDDS.Core.Export.Natives.Name.Factories;
using kCura.WinEDDS.Core.IO;
using kCura.WinEDDS.Service.Export;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	internal class StoppableExporterFactory : IExporterFactory
	{
		private readonly IInstanceSettingRepository _instanceSettingRepository;
		private readonly IAPILog _logger;
		private readonly JobHistoryErrorServiceProvider _jobHistoryErrorServiceProvider;
		private readonly IFileNameProvidersDictionaryBuilder _fileNameProvidersDictionaryBuilder;

		public StoppableExporterFactory(JobHistoryErrorServiceProvider jobHistoryErrorServiceProvider, IInstanceSettingRepository instanceSettingRepository, IHelper helper, IFileNameProvidersDictionaryBuilder fileNameProvidersDictionaryBuilder)
		{
			_jobHistoryErrorServiceProvider = jobHistoryErrorServiceProvider;
			_instanceSettingRepository = instanceSettingRepository;
			_fileNameProvidersDictionaryBuilder = fileNameProvidersDictionaryBuilder;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<StoppableExporterFactory>();
		}

		public IExporter Create(ExportDataContext exportDataContext)
		{
			var useCoreApiConfig = _instanceSettingRepository.GetConfigurationValue(Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION,
				Constants.REPLACE_WEB_API_WITH_EXPORT_CORE);
			var jobStopManager = _jobHistoryErrorServiceProvider?.JobHistoryErrorService.JobStopManager;
			var controller = new Controller();

			bool useCoreApi;
			IServiceFactory serviceFactory;
			if (bool.TryParse(useCoreApiConfig, out useCoreApi) && useCoreApi)
			{
				LogUsingRelativityCore();
				serviceFactory = new CoreServiceFactory(exportDataContext.ExportFile);
			}
			else
			{
				LogUsingWebApi();
				serviceFactory = new WebApiServiceFactory(exportDataContext.ExportFile);
			}
			var loadFileFormatterFactory = new ExportFileFormatterFactory(new ExtendedFieldNameProvider(exportDataContext.Settings));

			bool nameTextAndNativesAfterBegBates = exportDataContext.ExportFile.AreSettingsApplicableForProdBegBatesNameCheck();
			IDictionary<ExportNativeWithFilenameFrom, IFileNameProvider> fileNameProvidersDictionary = _fileNameProvidersDictionaryBuilder.Build(exportDataContext);

			var fileNameProviderContainerFactory = new FileNameProviderContainerFactory(fileNameProvidersDictionary);
			var fileNameProvider = fileNameProviderContainerFactory.Create(exportDataContext.ExportFile);

			var exporter = new ExtendedExporter(exportDataContext.ExportFile, controller, serviceFactory,
				loadFileFormatterFactory)
			{
				NameTextAndNativesAfterBegBates = nameTextAndNativesAfterBegBates, 
				FileHelper = new LongPathFileHelper(),
				DirectoryHelper = new LongPathDirectoryHelper(),
				FileNameProvider = fileNameProvider
			};
			return new StoppableExporter(exporter, controller, jobStopManager);
		}

		#region Logging

		private void LogUsingWebApi()
		{
			_logger.LogInformation("Exporter will be using WebAPI.");
		}

		private void LogUsingRelativityCore()
		{
			_logger.LogInformation("Exporter will be using Relativity.Core instead of WebAPI.");
		}

		#endregion
	}
}