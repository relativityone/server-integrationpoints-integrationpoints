using System;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.API;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	/// <summary>
	/// Implementation above the original FileRepository for creating and disposing IFileManager kepler proxy.
	/// It will be no longer required when we move all dependencies to the IoC container.
	/// </summary>
	public class DisposableFileRepository : IFileRepository
	{
		private readonly IServicesMgr _servicesMgr;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
		private readonly CreateFileRepositoryDelegate _createFileRepositoryDelegate;

		public delegate IFileRepository CreateFileRepositoryDelegate(
			IFileManager fileManager,
			IExternalServiceInstrumentationProvider instrumentationProvider
		);

		public DisposableFileRepository(
			IServicesMgr servicesMgr,
			IExternalServiceInstrumentationProvider instrumentationProvider,
			CreateFileRepositoryDelegate createFileRepositoryDelegate)
		{
			_servicesMgr = servicesMgr;
			_instrumentationProvider = instrumentationProvider;
			_createFileRepositoryDelegate = createFileRepositoryDelegate;
		}

		public FileResponse[] GetNativesForSearch(int workspaceID, int[] documentIDs)
		{
			using (IFileManager fileManager = CreateFileManagerProxy())
			{
				return _createFileRepositoryDelegate(fileManager, _instrumentationProvider)
					.GetNativesForSearch(workspaceID, documentIDs);
			}
		}

		public FileResponse[] GetNativesForProduction(int workspaceID, int productionID, int[] documentIDs)
		{
			using (IFileManager fileManager = CreateFileManagerProxy())
			{
				return _createFileRepositoryDelegate(fileManager, _instrumentationProvider)
					.GetNativesForProduction(workspaceID, productionID, documentIDs);
			}
		}

		public ProductionDocumentImageResponse[] GetImagesForProductionDocuments(int workspaceID, int productionID, int[] documentIDs)
		{
			using (IFileManager fileManager = CreateFileManagerProxy())
			{
				return _createFileRepositoryDelegate(fileManager, _instrumentationProvider)
					.GetImagesForProductionDocuments(workspaceID, productionID, documentIDs);
			}
		}

		public DocumentImageResponse[] GetImagesForDocuments(int workspaceID, int[] documentIDs)
		{
			using (IFileManager fileManager = CreateFileManagerProxy())
			{
				return _createFileRepositoryDelegate(fileManager, _instrumentationProvider)
					.GetImagesForDocuments(workspaceID, documentIDs);
			}
		}

		public FileResponse[] GetProducedImagesForDocument(int workspaceID, int documentID)
		{
			using (IFileManager fileManager = CreateFileManagerProxy())
			{
				return _createFileRepositoryDelegate(fileManager, _instrumentationProvider)
					.GetProducedImagesForDocument(workspaceID, documentID);
			}
		}

		public ExportProductionDocumentImageResponse[] GetImagesForExport(int workspaceID, int[] productionIDs, int[] documentIDs)
		{
			using (IFileManager fileManager = CreateFileManagerProxy())
			{
				return _createFileRepositoryDelegate(fileManager, _instrumentationProvider)
					.GetImagesForExport(workspaceID, productionIDs, documentIDs);
			}
		}

		private IFileManager CreateFileManagerProxy()
		{
			return _servicesMgr.CreateProxy<IFileManager>(ExecutionIdentity.CurrentUser);
		}
	}
}
