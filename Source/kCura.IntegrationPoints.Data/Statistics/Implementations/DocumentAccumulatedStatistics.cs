using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
	public class DocumentAccumulatedStatistics : IDocumentAccumulatedStatistics
	{
		private readonly IRelativityObjectManager _relativityObjectManager;
		private readonly INativeFileSizeStatistics _nativeFileSizeStatistics;
		private readonly IAPILog _logger;

		public DocumentAccumulatedStatistics(IRelativityObjectManager relativityObjectManager, INativeFileSizeStatistics nativeFileSizeStatistics, IAPILog logger)
		{
			_relativityObjectManager = relativityObjectManager;
			_nativeFileSizeStatistics = nativeFileSizeStatistics;
			_logger = logger;
		}

		public Task<DocumentsStatistics> GetNativesStatisticsForSavedSearchAsync(int workspaceId, int savedSearchId)
		{
			try
			{
				DocumentsStatistics statistics = new DocumentsStatistics();

				QueryRequest query = new DocumentQueryBuilder()
				.AddSavedSearchCondition(savedSearchId)
				.AddField(DocumentFieldsConstants.HasNativeFieldGuid)
				.Build();

				List<RelativityObject> documents = _relativityObjectManager.Query(query);
				
				statistics.DocumentsCount = documents.Count;
				statistics.TotalNativesCount = documents.Count(x => (bool)x[DocumentFieldsConstants.HasNativeFieldGuid].Value == true);
				statistics.TotalNativesSizeBytes = _nativeFileSizeStatistics.GetTotalFileSize(documents.Select(x => x.ArtifactID), workspaceId);

				return Task.FromResult(statistics);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception occurred while calculating natives statistics for Saved Search ID: {savedSearchId} in Workspace ID: {workspaceId}", savedSearchId, workspaceId);
				throw;
			}
		}

		public Task<DocumentsStatistics> GetImagesStatisticsForSavedSearchAsync(int workspaceId, int savedSearchId)
		{
			throw new System.NotImplementedException();
		}

		public Task<DocumentsStatistics> GetImagesStatisticsForProductionAsync(int workspaceId, int productionId)
		{
			throw new System.NotImplementedException();
		}
	}
}