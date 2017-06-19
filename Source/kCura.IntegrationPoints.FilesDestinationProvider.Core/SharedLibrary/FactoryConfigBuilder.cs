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
using kCura.WinEDDS.Service.Export;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	internal class FactoryConfigBuilder : IFactoryConfigBuilder
	{
		private readonly IAPILog _logger;

		private readonly IInstanceSettingRepository _instanceSettingRepository;
		private readonly JobHistoryErrorServiceProvider _jobHistoryErrorServiceProvider;
		private readonly IFileNameProvidersDictionaryBuilder _fileNameProvidersDictionaryBuilder;

		public FactoryConfigBuilder(IHelper helper, JobHistoryErrorServiceProvider jobHistoryErrorServiceProvider,
			IInstanceSettingRepository instanceSettingRepository, IFileNameProvidersDictionaryBuilder fileNameProvidersDictionaryBuilder)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ExtendedExporterFactory>();
			_jobHistoryErrorServiceProvider = jobHistoryErrorServiceProvider;
			_instanceSettingRepository = instanceSettingRepository;
			_fileNameProvidersDictionaryBuilder = fileNameProvidersDictionaryBuilder;
		}

		public ExporterFactoryConfig BuildFactoryConfig(ExportDataContext exportDataContext)
		{
			ExporterFactoryConfig config = new ExporterFactoryConfig();
			var useCoreApiConfig = _instanceSettingRepository.GetConfigurationValue(Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION,
				Constants.REPLACE_WEB_API_WITH_EXPORT_CORE);
			config.JobStopManager = _jobHistoryErrorServiceProvider?.JobHistoryErrorService.JobStopManager;
			config.Controller = new Controller();
			config.ServiceFactory = SetupServiceFactory(exportDataContext, useCoreApiConfig);
			config.LoadFileFormatterFactory = new ExportFileFormatterFactory(new ExtendedFieldNameProvider(exportDataContext.Settings));
			config.NameTextAndNativesAfterBegBates = exportDataContext.ExportFile.AreSettingsApplicableForProdBegBatesNameCheck();
			IDictionary<ExportNativeWithFilenameFrom, IFileNameProvider> fileNameProvidersDictionary = _fileNameProvidersDictionaryBuilder.Build(exportDataContext);

			var fileNameProviderContainerFactory = new FileNameProviderContainerFactory(fileNameProvidersDictionary);
			config.FileNameProvider = fileNameProviderContainerFactory.Create(exportDataContext.ExportFile);
			return config;
		}

		internal IServiceFactory SetupServiceFactory(ExportDataContext exportDataContext, string useCoreApiConfig)
		{
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
			return serviceFactory;
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