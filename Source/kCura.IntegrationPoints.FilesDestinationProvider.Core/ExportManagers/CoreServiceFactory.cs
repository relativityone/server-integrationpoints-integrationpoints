using System.Security.Claims;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.WinEDDS;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;
using Relativity.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	public class CoreServiceFactory : IServiceFactory
	{
		private readonly ExportFile _exportFile;

		public CoreServiceFactory(ExportFile exportFile)
		{
			_exportFile = exportFile;
		}

		public IAuditManager CreateAuditManager()
		{
			return new CoreAuditManager(GetBaseServiceContext());
		}

		public IExportFileDownloader CreateExportFileDownloader()
		{
			var destinationFolderPath = $"{_exportFile.CaseInfo.DocumentPath}\\EDDS{_exportFile.CaseInfo.ArtifactID}";
			return new FileDownloader(_exportFile.Credential, destinationFolderPath, _exportFile.CaseInfo.DownloadHandlerURL, _exportFile.CookieContainer,
				Settings.AuthenticationToken);
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
			return new CoreSearchManager(GetBaseServiceContext());
		}

		private BaseServiceContext GetBaseServiceContext()
		{
			return ClaimsPrincipal.Current.GetUnversionContext(_exportFile.CaseArtifactID);
		}
	}
}