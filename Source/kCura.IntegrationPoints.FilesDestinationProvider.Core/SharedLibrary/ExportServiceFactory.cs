using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    internal class ExportServiceFactory : IExportServiceFactory
    {
        private readonly CreateWebApiServiceFactoryDelegate _createWebApiServiceFactoryDelegate;
        private readonly CreateCoreServiceFactoryDelegate _createCoreServiceFactoryDelegate;
        private readonly IAPILog _logger;

        internal delegate WebApiServiceFactory CreateWebApiServiceFactoryDelegate(ExportFile exportFile);
        internal delegate CoreServiceFactory CreateCoreServiceFactoryDelegate(ExportFile exportFile, IServiceFactory webApiServiceFactory);

        public ExportServiceFactory(
            IAPILog logger,
            CreateWebApiServiceFactoryDelegate createWebApiServiceFactoryDelegate,
            CreateCoreServiceFactoryDelegate createCoreServiceFactoryDelegate)
        {
            _logger = logger.ForContext<ExportServiceFactory>();

            _createWebApiServiceFactoryDelegate = createWebApiServiceFactoryDelegate;
            _createCoreServiceFactoryDelegate = createCoreServiceFactoryDelegate;
        }

        public IServiceFactory Create(ExportDataContext exportDataContext)
        {
            WebApiServiceFactory webApiServiceFactory = _createWebApiServiceFactoryDelegate(exportDataContext.ExportFile);

            _logger.LogInformation("Exporter will be using Relativity.Core instead of WebAPI.");
            return _createCoreServiceFactoryDelegate(exportDataContext.ExportFile, webApiServiceFactory);
        }
    }
}
