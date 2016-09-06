using System.Security.Claims;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class CoreServiceFactory : IServiceFactory
	{
		private readonly ExportFile _exportFile;
		private readonly IServiceFactory _serviceFactory;

		public CoreServiceFactory(ExportFile exportFile)
		{
			_exportFile = exportFile;
			_serviceFactory = new WebApiServiceFactory(exportFile);
		}

		public IAuditManager CreateAuditManager()
		{
			return _serviceFactory.CreateAuditManager();
		}

		public IExportFileDownloader CreateExportFileDownloader()
		{
			return _serviceFactory.CreateExportFileDownloader();
		}

		public IExportManager CreateExportManager()
		{
			var baseContext = ClaimsPrincipal.Current.GetUnversionContext(_exportFile.CaseArtifactID);
			return new CoreExportManager(baseContext);
		}

		public IFieldManager CreateFieldManager()
		{
			return _serviceFactory.CreateFieldManager();
		}

		public IProductionManager CreateProductionManager()
		{
			return _serviceFactory.CreateProductionManager();
		}

		public ISearchManager CreateSearchManager()
		{
			return _serviceFactory.CreateSearchManager();
		}
	}
}