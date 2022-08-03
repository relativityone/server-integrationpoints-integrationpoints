using System;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
    internal class CoreServiceFactory : IServiceFactory
    {
        private readonly Func<IAuditManager> _auditManagerFactory;
        
        private readonly IServiceFactory _webApiServiceFactory;

        public CoreServiceFactory(Func<IAuditManager> auditManagerFactory, IServiceFactory webApiServiceFactory)
        {
            _auditManagerFactory = auditManagerFactory;
            _webApiServiceFactory = webApiServiceFactory;
        }

        public IAuditManager CreateAuditManager(Func<string> correlationIdFunc) => _auditManagerFactory();

        public IExportManager CreateExportManager(Func<string> correlationIdFunc) => _webApiServiceFactory.CreateExportManager(correlationIdFunc);

        public IFieldManager CreateFieldManager(Func<string> correlationIdFunc) => _webApiServiceFactory.CreateFieldManager(correlationIdFunc);

        public ISearchManager CreateSearchManager(Func<string> correlationIdFunc) => _webApiServiceFactory.CreateSearchManager(correlationIdFunc);

        public IProductionManager CreateProductionManager(Func<string> correlationIdFunc) => _webApiServiceFactory.CreateProductionManager(correlationIdFunc);
    }
}