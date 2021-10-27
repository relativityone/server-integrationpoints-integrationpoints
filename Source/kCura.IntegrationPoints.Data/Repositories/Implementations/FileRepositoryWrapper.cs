using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.Data.Toggles;
using kCura.WinEDDS.Service.Export;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class FileRepositoryWrapper : IFileRepository
	{
		private readonly IFileRepository _fileRepository;

		public FileRepositoryWrapper(IToggleProvider toggleProvider, Func<ISearchManager> searchManagerFactory, IServicesMgr servicesMgr,
			IExternalServiceInstrumentationProvider instrumentationProvider, IRetryHandlerFactory retryHandlerFactory)
		{
			_fileRepository = toggleProvider.IsEnabled<EnableKeplerizedImportAPIToggle>()
				? (IFileRepository) new FileRepository(servicesMgr, instrumentationProvider, retryHandlerFactory)
				: new WebAPIFileRepository(searchManagerFactory, instrumentationProvider, retryHandlerFactory);
		}

		public ILookup<int, ImageFile> GetImagesLocationForProductionDocuments(int workspaceID, int productionID, int[] documentIDs,
			ISearchManager searchManager = null)
		{
			return _fileRepository.GetImagesLocationForProductionDocuments(
				workspaceID, productionID, documentIDs, searchManager);
		}

		public ILookup<int, ImageFile> GetImagesLocationForDocuments(int workspaceID, int[] documentIDs, ISearchManager searchManager = null)
		{
			return _fileRepository.GetImagesLocationForDocuments(workspaceID, documentIDs, searchManager);
		}

		public List<FileDto> GetNativesForDocuments(int workspaceID, int[] documentIDs, ISearchManager searchManager = null)
		{
			return _fileRepository.GetNativesForDocuments(workspaceID, documentIDs, searchManager);
		}
	}
}
