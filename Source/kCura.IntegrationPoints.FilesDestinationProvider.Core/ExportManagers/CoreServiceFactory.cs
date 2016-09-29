using System.Security.Claims;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
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
			return new CoreAuditManager(GetBaseServiceContext());
		}

		public IExportFileDownloader CreateExportFileDownloader()
		{
			return _serviceFactory.CreateExportFileDownloader();
		}

		public IExportManager CreateExportManager()
		{
			return new CoreExportManager(GetBaseServiceContext());
		}

		public IFieldManager CreateFieldManager()
		{
			return new CoreFieldManager(GetBaseServiceContext());
		}

		public IProductionManager CreateProductionManager()
		{
			return new CoreProductionManager(GetBaseServiceContext());
		}

		public ISearchManager CreateSearchManager()
		{
			return _serviceFactory.CreateSearchManager();
		}

		private BaseServiceContext GetBaseServiceContext()
		{
			return ClaimsPrincipal.Current.GetUnversionContext(_exportFile.CaseArtifactID);
		}
	}
}