using System;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
    public class DocumentTotalStatistics : IDocumentTotalStatistics
    {
        private const string _FOR_SAVED_SEARCH_ERROR = "Failed to retrieve total documents count for saved search id: {savedSearchId}.";
        private const string _FOR_FOLDER_ERROR = "Failed to retrieve total documents count for folder: {folderId} and view: {viewId}.";
        private const string _FOR_PRODUCTION_ERROR = "Failed to retrieve total documents count for production set: {productionSetId}.";
        private readonly IAPILog _logger;
        private readonly IRelativityObjectManager _relativityObjectManager;

        public DocumentTotalStatistics(IHelper helper, IRelativityObjectManager relativityObjectManager)
        {
            _relativityObjectManager = relativityObjectManager;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<DocumentTotalStatistics>();
        }

        public long ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
        {
            try
            {
                QueryRequest query = new DocumentQueryBuilder().AddFolderCondition(folderId, viewId, includeSubFoldersTotals).NoFields().Build();
                return _relativityObjectManager.QueryTotalCount(query);
            }
            catch (Exception e)
            {
                _logger.LogError(e, _FOR_FOLDER_ERROR, folderId, viewId);
                throw;
            }
        }

        public long ForProduction(int workspaceArtifactId, int productionSetId)
        {
            try
            {
                QueryRequest query = new ProductionInformationQueryBuilder().AddProductionSetCondition(productionSetId).NoFields().Build();
                return _relativityObjectManager.QueryTotalCount(query);
            }
            catch (Exception e)
            {
                _logger.LogError(e, _FOR_PRODUCTION_ERROR, productionSetId);
                throw;
            }
        }

        public long ForSavedSearch(int workspaceArtifactId, int savedSearchId)
        {
            try
            {
                QueryRequest query = new DocumentQueryBuilder().AddSavedSearchCondition(savedSearchId).NoFields().Build();
                return _relativityObjectManager.QueryTotalCount(query);
            }
            catch (Exception e)
            {
                _logger.LogError(e, _FOR_SAVED_SEARCH_ERROR, savedSearchId);
                throw;
            }
        }
    }
}
