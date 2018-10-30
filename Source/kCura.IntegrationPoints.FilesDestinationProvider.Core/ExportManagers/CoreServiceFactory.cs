using System.Security.Claims;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Service;
using ICaseManager = kCura.WinEDDS.Service.Export.ICaseManager;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	public class CoreServiceFactory : IExtendedServiceFactory
	{
		private readonly ExportFile _exportFile;
		private readonly int _contextUserId;
		private readonly IHelper _helper;

		public CoreServiceFactory(IHelper helper, ExportFile exportFile, int contextUserId)
		{
			_helper = helper;
			_exportFile = exportFile;
			_contextUserId = contextUserId;
		}

		public IAuditManager CreateAuditManager()
		{
			var repositoryFactory = new RepositoryFactory(_helper, _helper.GetServicesManager());
			return new CoreAuditManager(repositoryFactory);
		}

		public IExportFileDownloader CreateExportFileDownloader()
		{
			var destinationFolderPath = $"{_exportFile.CaseInfo.DocumentPath}\\EDDS{_exportFile.CaseInfo.ArtifactID}";
			return new FileDownloader(_exportFile.Credential, destinationFolderPath, _exportFile.CaseInfo.DownloadHandlerURL, _exportFile.CookieContainer);
		}

		public IExportManager CreateExportManager()
		{
			return new CoreExportManager(GetBaseServiceContext(_exportFile.CaseArtifactID));
		}

		public IFieldManager CreateFieldManager()
		{
			var repositoryFactory = new RepositoryFactory(_helper, _helper.GetServicesManager());
			return new CoreFieldManager(repositoryFactory);
		}

		public IProductionManager CreateProductionManager()
		{
			return new CoreProductionManager(GetBaseServiceContext(_exportFile.CaseArtifactID));
		}

		public ISearchManager CreateSearchManager()
		{
			return new CoreSearchManager(GetBaseServiceContext(_exportFile.CaseArtifactID));
		}

		public ICaseManager CreateCaseManager()
		{
			return new CoreCaseManager(GetBaseServiceContext(-1));
		}
		
		private BaseServiceContext GetBaseServiceContext(int workspaceArtifactId)
		{
			BaseServiceContext bsc = ClaimsPrincipal.Current.GetUnversionContext(workspaceArtifactId);
			var loginManager = new LoginManager();
			
			return new ServiceContext(loginManager.GetLoginIdentity(_contextUserId), bsc.RequestOrigination, workspaceArtifactId);
		}
	}
}