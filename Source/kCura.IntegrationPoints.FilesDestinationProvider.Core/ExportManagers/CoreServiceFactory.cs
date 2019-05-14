﻿using System.Security.Claims;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity.Core;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	public class CoreServiceFactory : IServiceFactory
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IViewFieldRepository _viewFieldRepository;
		private readonly IFileFieldRepository _fileFieldRepository;
		private readonly IFileRepository _fileRepository;
		private readonly IViewRepository _viewRepository;
		private readonly ExportFile _exportFile;
		private readonly IServiceFactory _webApiServiceFactory;
		private readonly int _contextUserId;

		public CoreServiceFactory(
			IRepositoryFactory repositoryFactory,
			IViewFieldRepository viewFieldRepository,
			IFileFieldRepository fileFieldRepository,
			IFileRepository fileRepository,
			IViewRepository viewRepository,
			ExportFile exportFile,
			IServiceFactory webApiServiceFactory,
			int contextUserId)
		{
			_repositoryFactory = repositoryFactory;
			_viewFieldRepository = viewFieldRepository;
			_fileFieldRepository = fileFieldRepository;
			_fileRepository = fileRepository;
			_viewRepository = viewRepository;
			_exportFile = exportFile;
			_contextUserId = contextUserId;
			_webApiServiceFactory = webApiServiceFactory;
		}

		public IAuditManager CreateAuditManager() => new CoreAuditManager(_repositoryFactory, _contextUserId);

		public IExportManager CreateExportManager() => new CoreExportManager(GetBaseServiceContext(_exportFile.CaseArtifactID));

		public IFieldManager CreateFieldManager() => new CoreFieldManager(_repositoryFactory);

		public ISearchManager CreateSearchManager() => new CoreSearchManager(
			_fileRepository, 
			_fileFieldRepository, 
			_viewFieldRepository,
			_viewRepository
		);

		public IProductionManager CreateProductionManager()
		{
			return _webApiServiceFactory.CreateProductionManager();
		}

		public IExportFileDownloader CreateExportFileDownloader()
		{
			string destinationFolderPath = $"{_exportFile.CaseInfo.DocumentPath}\\EDDS{_exportFile.CaseInfo.ArtifactID}";
			return new FileDownloader(_exportFile.Credential, destinationFolderPath, _exportFile.CaseInfo.DownloadHandlerURL, _exportFile.CookieContainer);
		}
		
		private BaseServiceContext GetBaseServiceContext(int workspaceArtifactId)
		{
			BaseServiceContext bsc = ClaimsPrincipal.Current.GetUnversionContext(workspaceArtifactId);
			var loginManager = new LoginManager();
			
			return new ServiceContext(loginManager.GetLoginIdentity(_contextUserId), bsc.RequestOrigination, workspaceArtifactId);
		}
	}
}