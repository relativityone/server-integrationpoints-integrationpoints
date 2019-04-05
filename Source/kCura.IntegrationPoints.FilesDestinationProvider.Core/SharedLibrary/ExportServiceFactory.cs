using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using kCura.WinEDDS.Service.Export;
using Relativity.API;
using IFileRepository = kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.IFileRepository;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class ExportServiceFactory : IExportServiceFactory
	{
		private readonly CurrentUser _contextUser;
		private readonly IInstanceSettingRepository _instanceSettingRepository;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IViewFieldRepository _viewFieldRepository;
		private readonly IFileRepository _fileRepository;
		private readonly IFileFieldRepository _fileFieldRepository;
		private readonly IAPILog _logger;

		public ExportServiceFactory(IAPILog logger,
			IInstanceSettingRepository instanceSettingRepository,
			IRepositoryFactory repositoryFactory,
			IFileRepository fileRepository,
			IFileFieldRepository fileFieldRepository, 
			IViewFieldRepository viewFieldRepository,
			CurrentUser contextUser)
		{
			_logger = logger.ForContext<ExportServiceFactory>();
			_instanceSettingRepository = instanceSettingRepository;
			_repositoryFactory = repositoryFactory;
			_fileRepository = fileRepository;
			_fileFieldRepository = fileFieldRepository;
			_viewFieldRepository = viewFieldRepository;
			_contextUser = contextUser;
		}

		public IServiceFactory Create(ExportDataContext exportDataContext)
		{
			if (UseCoreApi())
			{
				LogUsingRelativityCore();
				return new CoreServiceFactory(
					_repositoryFactory, 
					_viewFieldRepository, 
					_fileFieldRepository, 
					_fileRepository, 
					exportDataContext.ExportFile, 
					_contextUser.ID
				);
			}

			LogUsingWebApi();
			return new WebApiServiceFactory(exportDataContext.ExportFile);
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