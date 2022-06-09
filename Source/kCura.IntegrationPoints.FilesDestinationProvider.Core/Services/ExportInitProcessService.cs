using System;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Services
{
	public class ExportInitProcessService : IExportInitProcessService
	{
		private readonly IRdoStatistics _rdoStatistics;
		private readonly IDocumentTotalStatistics _documentStatistics;
		private readonly IAPILog _logger;
		
		public ExportInitProcessService(IHelper helper, IDocumentTotalStatistics documentStatistics, IRdoStatistics rdoStatistics)
		{
			_documentStatistics = documentStatistics;
			_rdoStatistics = rdoStatistics;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportInitProcessService>();
		}

		public long CalculateDocumentCountToTransfer(ExportUsingSavedSearchSettings exportSettings, int artifactTypeId)
		{
			_logger.LogInformation("Start retrieving Total Document count for {exportSettings.ExportType} export type...", exportSettings.ExportType);

			ExportSettings.ExportType exportType = GetExportType(exportSettings);

			long docsCount;

			if (artifactTypeId == (int) ArtifactType.Document)
			{
				docsCount = GetTotalDocCount(exportSettings, exportType, _documentStatistics);
			}
			else
			{
				docsCount = GetTotalRdoCount(exportSettings, artifactTypeId, _rdoStatistics);
			}


			long extractedIndex = Math.Min(docsCount, Math.Abs(exportSettings.StartExportAtRecord - 1));
			long retValue = Math.Max(docsCount - extractedIndex, 0);

			_logger.LogInformation("Calculated Total Document count: {retValue}", retValue);
			return retValue;
		}

		private static long GetTotalDocCount(ExportUsingSavedSearchSettings exportSettings, ExportSettings.ExportType exportType, IDocumentTotalStatistics documentTotalStatistics)
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
			return rdoStatistics.ForView(artifactTypeId, exportSettings.ViewId);
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