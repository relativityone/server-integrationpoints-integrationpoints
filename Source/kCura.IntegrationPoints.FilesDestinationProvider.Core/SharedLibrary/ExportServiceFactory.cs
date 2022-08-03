using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    internal class ExportServiceFactory : IExportServiceFactory
    {
        private readonly IInstanceSettingRepository _instanceSettingRepository;

        private readonly CreateWebApiServiceFactoryDelegate _createWebApiServiceFactoryDelegate;
        private readonly CreateCoreServiceFactoryDelegate _createCoreServiceFactoryDelegate;

        private readonly IAPILog _logger;

        internal delegate WebApiServiceFactory CreateWebApiServiceFactoryDelegate(ExportFile exportFile);
        internal delegate CoreServiceFactory CreateCoreServiceFactoryDelegate(ExportFile exportFile, IServiceFactory webApiServiceFactory);

        public ExportServiceFactory(
            IAPILog logger,
            IInstanceSettingRepository instanceSettingRepository,
            CreateWebApiServiceFactoryDelegate createWebApiServiceFactoryDelegate,
            CreateCoreServiceFactoryDelegate createCoreServiceFactoryDelegate)
        {
            _logger = logger.ForContext<ExportServiceFactory>();
            _instanceSettingRepository = instanceSettingRepository;

            _createWebApiServiceFactoryDelegate = createWebApiServiceFactoryDelegate;
            _createCoreServiceFactoryDelegate = createCoreServiceFactoryDelegate;
        }

        public IServiceFactory Create(ExportDataContext exportDataContext)
        {
            WebApiServiceFactory webApiServiceFactory = _createWebApiServiceFactoryDelegate(exportDataContext.ExportFile);
            if (UseCoreApi())
            {
                LogUsingRelativityCore();
                return _createCoreServiceFactoryDelegate(exportDataContext.ExportFile, webApiServiceFactory);
            }

            LogUsingWebApi();
            return webApiServiceFactory;
        }

        private bool UseCoreApi()
        {
            string value = _instanceSettingRepository.GetConfigurationValue(
                section: Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION,
                name: Constants.REPLACE_WEB_API_WITH_EXPORT_CORE);

            bool useCoreApi;
            bool.TryParse(value, out useCoreApi);

            return useCoreApi;
        }

        private void LogUsingWebApi()
        {
            _logger.LogInformation("Exporter will be using WebAPI.");
        }

        private void LogUsingRelativityCore()
        {
            _logger.LogInformation("Exporter will be using Relativity.Core instead of WebAPI.");
        }
    }
}