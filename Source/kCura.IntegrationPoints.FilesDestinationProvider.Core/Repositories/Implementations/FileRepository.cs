using System.Runtime.CompilerServices;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.Implementations
{
	public class FileRepository : IFileRepository
	{
		private readonly IFileManager _fileManager;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;

		public FileRepository(
			IFileManager fileManager, 
			IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_fileManager = fileManager;
			_instrumentationProvider = instrumentationProvider;
		}

		public FileResponse[] GetNativesForSearchAsync(int workspaceID, int[] documentIDs)
		{
			return CreateInstrumentation().Execute(
				() => _fileManager.GetNativesForSearchAsync(workspaceID, documentIDs)
					.GetAwaiter()
					.GetResult()
			);
		}

		public FileResponse[] GetNativesForProductionAsync(
			int workspaceID, 
			int productionID, 
			int[] documentIDs)
		{
			return CreateInstrumentation().Execute(
				() => _fileManager.GetNativesForProductionAsync(workspaceID, productionID, documentIDs)
					.GetAwaiter()
					.GetResult()
			);
		}

		public ProductionDocumentImageResponse[] GetImagesForProductionDocumentsAsync(
			int workspaceID, 
			int productionID,
			int[] documentIDs)
		{
			return CreateInstrumentation().Execute(
				() => _fileManager.GetImagesForProductionDocumentsAsync(workspaceID, productionID, documentIDs)
					.GetAwaiter()
					.GetResult()
			);
		}

		public DocumentImageResponse[] GetImagesForDocumentsAsync(int workspaceID, int[] documentIDs)
		{
			return CreateInstrumentation().Execute(
				() => _fileManager.GetImagesForDocumentsAsync(workspaceID, documentIDs)
					.GetAwaiter()
					.GetResult()
			);
		}

		public FileResponse[] GetProducedImagesForDocumentAsync(int workspaceID, int documentID)
		{
			return CreateInstrumentation().Execute(
				() => _fileManager.GetProducedImagesForDocumentAsync(workspaceID, documentID)
					.GetAwaiter()
					.GetResult()
			);
		}

		public ExportProductionDocumentImageResponse[] GetImagesForExportAsync(int workspaceID, int[] productionIDs, int[] documentIDs)
		{
			return CreateInstrumentation().Execute(
				() => _fileManager.GetImagesForExportAsync(workspaceID, productionIDs, documentIDs)
					.GetAwaiter()
					.GetResult()
			);
		}
		private IExternalServiceSimpleInstrumentation CreateInstrumentation([CallerMemberName] string methodName = "")
		{
			return _instrumentationProvider.CreateSimple(
				ExternalServiceTypes.KEPLER,
				nameof(IFileManager),
				methodName
			);
		}
	}
}
