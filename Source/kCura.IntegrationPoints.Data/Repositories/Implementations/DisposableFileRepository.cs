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

		public DisposableFileRepository(
			IServicesMgr servicesMgr,
			IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_servicesMgr = servicesMgr;
			_instrumentationProvider = instrumentationProvider;
		}

		public FileResponse[] GetNativesForSearch(int workspaceID, int[] documentIDs)
		{
			using (IFileManager fileManager = CreateFileManagerProxy())
			{
				return CreateFileRepository(fileManager)
					.GetNativesForSearch(workspaceID, documentIDs);
			}
		}

		public FileResponse[] GetNativesForProduction(int workspaceID, int productionID, int[] documentIDs)
		{
			using (IFileManager fileManager = CreateFileManagerProxy())
			{
				return CreateFileRepository(fileManager)
					.GetNativesForProduction(workspaceID, productionID, documentIDs);
			}
		}

		public ProductionDocumentImageResponse[] GetImagesForProductionDocuments(int workspaceID, int productionID, int[] documentIDs)
		{
			using (IFileManager fileManager = CreateFileManagerProxy())
			{
				return CreateFileRepository(fileManager)
					.GetImagesForProductionDocuments(workspaceID, productionID, documentIDs);
			}
		}

		public DocumentImageResponse[] GetImagesForDocuments(int workspaceID, int[] documentIDs)
		{
			using (IFileManager fileManager = CreateFileManagerProxy())
			{
				return CreateFileRepository(fileManager)
					.GetImagesForDocuments(workspaceID, documentIDs);
			}
		}

		public FileResponse[] GetProducedImagesForDocument(int workspaceID, int documentID)
		{
			using (IFileManager fileManager = CreateFileManagerProxy())
			{
				return CreateFileRepository(fileManager)
					.GetProducedImagesForDocument(workspaceID, documentID);
			}
		}

		public ExportProductionDocumentImageResponse[] GetImagesForExport(int workspaceID, int[] productionIDs, int[] documentIDs)
		{
			using (IFileManager fileManager = CreateFileManagerProxy())
			{
				return CreateFileRepository(fileManager)
					.GetImagesForExport(workspaceID, productionIDs, documentIDs);
			}
		}

		private IFileManager CreateFileManagerProxy()
		{
			return _servicesMgr.CreateProxy<IFileManager>(ExecutionIdentity.CurrentUser);
		}

		private IFileRepository CreateFileRepository(IFileManager fileManager)
		{
			return new FileRepository(fileManager, _instrumentationProvider);
		}
	}
}
