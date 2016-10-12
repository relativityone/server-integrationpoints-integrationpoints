using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Extensions;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.Windows.Process;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	internal class StoppableExporterFactory : IExporterFactory
	{
		private readonly IInstanceSettingRepository _instanceSettingRepository;
		private readonly JobHistoryErrorServiceProvider _jobHistoryErrorServiceProvider;

		public StoppableExporterFactory(JobHistoryErrorServiceProvider jobHistoryErrorServiceProvider, IInstanceSettingRepository instanceSettingRepository)
		{
			_jobHistoryErrorServiceProvider = jobHistoryErrorServiceProvider;
			_instanceSettingRepository = instanceSettingRepository;
		}

		public IExporter Create(ExportFile exportFile)
		{
			var useCoreApiConfig = _instanceSettingRepository.GetConfigurationValue(Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION,
				Constants.REPLACE_WEB_API_WITH_EXPORT_CORE);
			var jobStopManager = _jobHistoryErrorServiceProvider?.JobHistoryErrorService.JobStopManager;
			var controller = new Controller();

			bool useCoreApi;
			IServiceFactory serviceFactory;
			if (bool.TryParse(useCoreApiConfig, out useCoreApi) && useCoreApi)
			{
				serviceFactory = new CoreServiceFactory(exportFile);
			}
			else
			{
				serviceFactory = new WebApiServiceFactory(exportFile);
			}
			var exporter = new Exporter(exportFile, controller, serviceFactory)
			{
				NameTextAndNativesAfterBegBates = exportFile.AreSettingsApplicableForProdBegBatesNameCheck()
			};
			return new StoppableExporter(exporter, controller, jobStopManager);
		}
	}
}