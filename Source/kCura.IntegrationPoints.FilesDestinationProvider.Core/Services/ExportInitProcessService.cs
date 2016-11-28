using System;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.API;
using ExportSettings = kCura.IntegrationPoints.Core.Models.ExportSettings;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ExportInitProcessService  : IExportInitProcessService
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAPILog _logger;

		public ExportInitProcessService(IHelper helper, IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportInitProcessService>();
		}

		public int CalculateDocumentCountToTransfer(ExportUsingSavedSearchSettings exportSettings, int artifactTypeId)
		{
			_logger.LogDebug("Start retrieving Total Document count for {exportSettings.ExportType} export type...", exportSettings.ExportType);

			ExportSettings.ExportType exportType = GetExportType(exportSettings);

			IDocumentTotalsRepository documentTotalsRepository = _repositoryFactory.GetDocumentTotalsRepository(exportSettings.SourceWorkspaceArtifactId);

			int docsCount = GetTotalDocCount(exportSettings, exportType, artifactTypeId, documentTotalsRepository);

			int extractedIndex = Math.Min(docsCount, Math.Abs(exportSettings.StartExportAtRecord - 1));
			int retValue = Math.Max(docsCount - extractedIndex, 0);

			_logger.LogDebug("Calculated Total Document count: {retValue}", retValue);
			return retValue;
		}

		private static int GetTotalDocCount(ExportUsingSavedSearchSettings exportSettings, ExportSettings.ExportType exportType, 
			int artifactTypeId, IDocumentTotalsRepository documentTotalsRepository)
		{
			switch (exportType)
			{
				case ExportSettings.ExportType.Folder:
				case ExportSettings.ExportType.FolderAndSubfolders:
					return artifactTypeId == (int)ArtifactType.Document
						? documentTotalsRepository.GetFolderTotalDocsCount(exportSettings.FolderArtifactId, exportSettings.ViewId,
							exportType == ExportSettings.ExportType.FolderAndSubfolders)
						: documentTotalsRepository.GetRdosCount(artifactTypeId, exportSettings.ViewId);
				case ExportSettings.ExportType.ProductionSet:
					return documentTotalsRepository.GetProductionDocsCount(exportSettings.ProductionId);
				default:
					return documentTotalsRepository.GetSavedSearchTotalDocsCount(exportSettings.SavedSearchArtifactId);
			}
		}

		private ExportSettings.ExportType GetExportType(ExportUsingSavedSearchSettings exportSettings)
		{
			try
			{
				ExportSettings.ExportType exportType;
				EnumHelper.Parse(exportSettings.ExportType, out exportType);
				return exportType;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Undefined export type found: {exportSettings.ExportType}", exportSettings.ExportType);
				throw;
			}
		}
	}
}
