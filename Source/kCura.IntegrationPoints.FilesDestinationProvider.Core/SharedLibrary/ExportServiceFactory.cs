using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class ExportServiceFactory : IExportServiceFactory
	{
		private readonly CurrentUser _contextUser;
		private readonly IInstanceSettingRepository _instanceSettingRepository;
		private readonly IAPILog _logger;
		private readonly IHelper _helper;

		public ExportServiceFactory(IHelper helper, IInstanceSettingRepository instanceSettingRepository, CurrentUser contextUser)
		{
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportServiceFactory>();
			_instanceSettingRepository = instanceSettingRepository;
			_contextUser = contextUser;
		}

		public IExtendedServiceFactory Create(ExportDataContext exportDataContext)
		{
			if (UseCoreApi())
			{
				LogUsingRelativityCore();
				return new CoreServiceFactory(_helper, exportDataContext.ExportFile, _contextUser.ID);
			}

			LogUsingWebApi();
			return new ExtendedWebApiServiceFactory(exportDataContext.ExportFile);
		}

		private bool UseCoreApi()
		{
			string value = _instanceSettingRepository.GetConfigurationValue(Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION,
				Constants.REPLACE_WEB_API_WITH_EXPORT_CORE);

			bool useCoreApi;
			bool.TryParse(value, out useCoreApi);

			return useCoreApi;
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