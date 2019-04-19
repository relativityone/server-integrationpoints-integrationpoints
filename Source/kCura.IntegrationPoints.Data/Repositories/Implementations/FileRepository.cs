using System;
using System.Linq;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
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

		public FileResponse[] GetNativesForSearch(int workspaceID, int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return Enumerable.Empty<FileResponse>().ToArray();
			}

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(IFileManager.GetNativesForSearchAsync)
			);

			return instrumentation.ExecuteAsync(
				() => _fileManager.GetNativesForSearchAsync(workspaceID, documentIDs)
			)
			.GetAwaiter()
			.GetResult();
		}

		public FileResponse[] GetNativesForProduction(
			int workspaceID, 
			int productionID, 
			int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return Enumerable.Empty<FileResponse>().ToArray();
			}


			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(IFileManager.GetNativesForProductionAsync)
			);
			return instrumentation.ExecuteAsync(
				() => _fileManager.GetNativesForProductionAsync(workspaceID, productionID, documentIDs)
			)
			.GetAwaiter()
			.GetResult();
		}

		public ProductionDocumentImageResponse[] GetImagesForProductionDocuments(
			int workspaceID, 
			int productionID,
			int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return Enumerable.Empty<ProductionDocumentImageResponse>().ToArray();
			}

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(IFileManager.GetImagesForProductionDocumentsAsync)
			);

			return instrumentation.ExecuteAsync(
				() => _fileManager.GetImagesForProductionDocumentsAsync(workspaceID, productionID, documentIDs)
			)
			.GetAwaiter()
			.GetResult();
		}

		public DocumentImageResponse[] GetImagesForDocuments(int workspaceID, int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return Enumerable.Empty<DocumentImageResponse>().ToArray();
			}

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(IFileManager.GetImagesForDocumentsAsync)
			);

			return instrumentation.ExecuteAsync(
				() => _fileManager.GetImagesForDocumentsAsync(workspaceID, documentIDs)
			)
			.GetAwaiter()
			.GetResult();
		}

		public FileResponse[] GetProducedImagesForDocument(int workspaceID, int documentID)
		{
			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(IFileManager.GetProducedImagesForDocumentAsync)
			);

			return instrumentation.ExecuteAsync(
				() => _fileManager.GetProducedImagesForDocumentAsync(workspaceID, documentID)
			)
			.GetAwaiter()
			.GetResult();
		}

		public ExportProductionDocumentImageResponse[] GetImagesForExport(int workspaceID, int[] productionIDs, int[] documentIDs)
		{
			ThrowWhenNullArgument(productionIDs, nameof(productionIDs));
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!productionIDs.Any() || !documentIDs.Any())
			{
				return Enumerable.Empty<ExportProductionDocumentImageResponse>().ToArray();
			}

			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(IFileManager.GetImagesForExportAsync)
			);

			return instrumentation.ExecuteAsync(
				() => _fileManager.GetImagesForExportAsync(workspaceID, productionIDs, documentIDs)
			)
			.GetAwaiter()
			.GetResult();
		}

		private void ThrowWhenNullArgument<T>(T argument, string argumentName)
		{
			if (argument == null)
			{
				throw new ArgumentNullException(argumentName);
			}
		}

		private IExternalServiceSimpleInstrumentation CreateInstrumentation(string operationName)
		{
			return _instrumentationProvider.CreateSimple(
				ExternalServiceTypes.KEPLER,
				nameof(IFileManager),
				operationName
			);
		}
	}
}
