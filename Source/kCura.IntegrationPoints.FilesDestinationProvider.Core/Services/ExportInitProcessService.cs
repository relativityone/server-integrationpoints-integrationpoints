using System;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

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

		public int CalculateDocumentCountToTransfer(ExportUsingSavedSearchSettings exportSettings)
		{
			_logger.LogDebug("Start retrieving Total Document count for {exportSettings.ExportType} export type...", exportSettings.ExportType);

			ExportSettings.ExportType exportType = GetExportType(exportSettings);

			IDocumentTotalsRepository documentTotalsRepository = _repositoryFactory.GetDocumentTotalsRepository(exportSettings.SourceWorkspaceArtifactId);

			int docsCount = GetTotalDocCount(exportSettings, exportType, documentTotalsRepository);

			int extractedIndex = Math.Min(docsCount, Math.Abs(exportSettings.StartExportAtRecord - 1));
			int retValue = Math.Max(docsCount - extractedIndex, 0);

			_logger.LogDebug("Calculated Total Document count: {retValue}", retValue);
			return retValue;
		}

		private static int GetTotalDocCount(ExportUsingSavedSearchSettings exportSettings, ExportSettings.ExportType exportType, IDocumentTotalsRepository documentTotalsRepository)
		{
			switch (exportType)
			{
				case ExportSettings.ExportType.Folder:
				case ExportSettings.ExportType.FolderAndSubfolders:
					return documentTotalsRepository.GetFolderTotalDocsCount(exportSettings.FolderArtifactId,
						exportSettings.ViewId, exportType == ExportSettings.ExportType.FolderAndSubfolders);
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
