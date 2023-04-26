using System;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	internal class CoreServiceFactory : IServiceFactory
	{
		private readonly Func<IAuditManager> _auditManagerFactory;

		private readonly ExportFile _exportFile;
		private readonly IServiceFactory _webApiServiceFactory;

		public CoreServiceFactory(
			Func<IAuditManager> auditManagerFactory,
			ExportFile exportFile,
			IServiceFactory webApiServiceFactory)
		{
			_auditManagerFactory = auditManagerFactory;

			_exportFile = exportFile;
			_webApiServiceFactory = webApiServiceFactory;
		}

		public IAuditManager CreateAuditManager() => _auditManagerFactory();

		public IExportManager CreateExportManager() => _webApiServiceFactory.CreateExportManager();

		public IFieldManager CreateFieldManager() => _webApiServiceFactory.CreateFieldManager();

		public ISearchManager CreateSearchManager() => _webApiServiceFactory.CreateSearchManager();

		public IProductionManager CreateProductionManager() => _webApiServiceFactory.CreateProductionManager();
	}
}