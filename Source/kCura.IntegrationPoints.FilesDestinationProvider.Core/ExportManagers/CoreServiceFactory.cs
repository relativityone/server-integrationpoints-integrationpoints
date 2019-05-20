using System;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers.Factories;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	internal class CoreServiceFactory : IServiceFactory
	{
		private readonly Func<IAuditManager> _auditManagerFactory;
		private readonly Func<IFieldManager> _fieldManagerFactory;
		private readonly Func<ISearchManager> _searchManagerFactory;
		private readonly IExportFileDownloaderFactory _exportFileDownloaderFactory;

		private readonly ExportFile _exportFile;
		private readonly IServiceFactory _webApiServiceFactory;

		public CoreServiceFactory(
			Func<IAuditManager> auditManagerFactory,
			Func<IFieldManager> fieldManagerFactory,
			Func<ISearchManager> searchManagerFactory,
			IExportFileDownloaderFactory exportFileDownloaderFactory,
			ExportFile exportFile,
			IServiceFactory webApiServiceFactory)
		{
			_auditManagerFactory = auditManagerFactory;
			_fieldManagerFactory = fieldManagerFactory;
			_searchManagerFactory = searchManagerFactory;
			_exportFileDownloaderFactory = exportFileDownloaderFactory;

			_exportFile = exportFile;
			_webApiServiceFactory = webApiServiceFactory;
		}

		public IAuditManager CreateAuditManager() => _auditManagerFactory();

		public IExportManager CreateExportManager() => _webApiServiceFactory.CreateExportManager();

		public IFieldManager CreateFieldManager() => _fieldManagerFactory();

		public ISearchManager CreateSearchManager() => _searchManagerFactory();

		public IProductionManager CreateProductionManager() => _webApiServiceFactory.CreateProductionManager();

		public IExportFileDownloader CreateExportFileDownloader() => _exportFileDownloaderFactory.Create(_exportFile);
	}
}