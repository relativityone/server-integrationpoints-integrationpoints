﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
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

			return CreateInstrumentation().Execute(
				() => _fileManager.GetNativesForSearchAsync(workspaceID, documentIDs)
					.GetAwaiter()
					.GetResult()
			);
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

			return CreateInstrumentation().Execute(
				() => _fileManager.GetNativesForProductionAsync(workspaceID, productionID, documentIDs)
					.GetAwaiter()
					.GetResult()
			);
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

			return CreateInstrumentation().Execute(
				() => _fileManager.GetImagesForProductionDocumentsAsync(workspaceID, productionID, documentIDs)
					.GetAwaiter()
					.GetResult()
			);
		}

		public DocumentImageResponse[] GetImagesForDocuments(int workspaceID, int[] documentIDs)
		{
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!documentIDs.Any())
			{
				return Enumerable.Empty<DocumentImageResponse>().ToArray();
			}

			return CreateInstrumentation().Execute(
				() => _fileManager.GetImagesForDocumentsAsync(workspaceID, documentIDs)
					.GetAwaiter()
					.GetResult()
			);
		}

		public FileResponse[] GetProducedImagesForDocument(int workspaceID, int documentID)
		{
			return CreateInstrumentation().Execute(
				() => _fileManager.GetProducedImagesForDocumentAsync(workspaceID, documentID)
					.GetAwaiter()
					.GetResult()
			);
		}

		public ExportProductionDocumentImageResponse[] GetImagesForExport(int workspaceID, int[] productionIDs, int[] documentIDs)
		{
			ThrowWhenNullArgument(productionIDs, nameof(productionIDs));
			ThrowWhenNullArgument(documentIDs, nameof(documentIDs));

			if (!productionIDs.Any() || !documentIDs.Any())
			{
				return Enumerable.Empty<ExportProductionDocumentImageResponse>().ToArray();
			}

			return CreateInstrumentation().Execute(
				() => _fileManager.GetImagesForExportAsync(workspaceID, productionIDs, documentIDs)
					.GetAwaiter()
					.GetResult()
			);
		}

		private void ThrowWhenNullArgument<T>(T argument, string argumentName)
		{
			if (argument == null)
			{
				throw new ArgumentNullException(argumentName);
			}
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
