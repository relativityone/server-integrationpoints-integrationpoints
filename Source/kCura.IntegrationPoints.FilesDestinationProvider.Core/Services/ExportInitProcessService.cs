using System;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ExportInitProcessService : IExportInitProcessService
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

			int docsCount;

			if (artifactTypeId == (int) ArtifactType.Document)
			{
				IDocumentTotalStatistics documentTotalStatistics = _repositoryFactory.GetDocumentTotalStatistics();
				docsCount = GetTotalDocCount(exportSettings, exportType, documentTotalStatistics);
			}
			else
			{
				IRdoStatistics rdoStatistics = _repositoryFactory.GetRdoStatistics();
				docsCount = GetTotalRdoCount(exportSettings, artifactTypeId, rdoStatistics);
			}


			int extractedIndex = Math.Min(docsCount, Math.Abs(exportSettings.StartExportAtRecord - 1));
			int retValue = Math.Max(docsCount - extractedIndex, 0);

			_logger.LogDebug("Calculated Total Document count: {retValue}", retValue);
			return retValue;
		}

		private static int GetTotalDocCount(ExportUsingSavedSearchSettings exportSettings, ExportSettings.ExportType exportType, IDocumentTotalStatistics documentTotalStatistics)
		{
			switch (exportType)
			{
				case ExportSettings.ExportType.Folder:
				case ExportSettings.ExportType.FolderAndSubfolders:
					return documentTotalStatistics.ForFolder(exportSettings.SourceWorkspaceArtifactId, exportSettings.FolderArtifactId, exportSettings.ViewId,
						exportType == ExportSettings.ExportType.FolderAndSubfolders);
				case ExportSettings.ExportType.ProductionSet:
					return documentTotalStatistics.ForProduction(exportSettings.SourceWorkspaceArtifactId, exportSettings.ProductionId);
				default:
					return documentTotalStatistics.ForSavedSearch(exportSettings.SourceWorkspaceArtifactId, exportSettings.SavedSearchArtifactId);
			}
		}

		private int GetTotalRdoCount(ExportUsingSavedSearchSettings exportSettings, int artifactTypeId, IRdoStatistics rdoStatistics)
		{
			return rdoStatistics.ForView(exportSettings.SourceWorkspaceArtifactId, artifactTypeId, exportSettings.ViewId);
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