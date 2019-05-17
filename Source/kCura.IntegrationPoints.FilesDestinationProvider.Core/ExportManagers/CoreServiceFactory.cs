using System;
using System.Security.Claims;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers.Factories;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity.Core;
using Relativity.Core.Service;

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
		private readonly CurrentUser _currentUser;

		public CoreServiceFactory(
			Func<IAuditManager> auditManagerFactory,
			Func<IFieldManager> fieldManagerFactory,
			Func<ISearchManager> searchManagerFactory,
			IExportFileDownloaderFactory exportFileDownloaderFactory,
			ExportFile exportFile,
			IServiceFactory webApiServiceFactory,
			CurrentUser currentUser // TODO it will be removed REL-241387
			)
		{
			_auditManagerFactory = auditManagerFactory;
			_fieldManagerFactory = fieldManagerFactory;
			_searchManagerFactory = searchManagerFactory;
			_exportFileDownloaderFactory = exportFileDownloaderFactory;

			_exportFile = exportFile;
			_currentUser = currentUser;
			_webApiServiceFactory = webApiServiceFactory;
		}

		public IAuditManager CreateAuditManager() => _auditManagerFactory();

		public IExportManager CreateExportManager() => new CoreExportManager(GetBaseServiceContext(_exportFile.CaseArtifactID)); // TODO It will be changed: REL-241387

		public IFieldManager CreateFieldManager() => _fieldManagerFactory();

		public ISearchManager CreateSearchManager() => _searchManagerFactory();

		public IProductionManager CreateProductionManager() => _webApiServiceFactory.CreateProductionManager();

		public IExportFileDownloader CreateExportFileDownloader() => _exportFileDownloaderFactory.Create(_exportFile);

		private BaseServiceContext GetBaseServiceContext(int workspaceArtifactId) // TODO it will be removed REL-241387
		{
			BaseServiceContext bsc = ClaimsPrincipal.Current.GetUnversionContext(workspaceArtifactId);
			var loginManager = new LoginManager();

			return new ServiceContext(loginManager.GetLoginIdentity(_currentUser.ID), bsc.RequestOrigination, workspaceArtifactId);
		}
	}
}