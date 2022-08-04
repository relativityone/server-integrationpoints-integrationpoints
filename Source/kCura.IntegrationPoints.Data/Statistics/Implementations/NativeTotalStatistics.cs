using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
    public class NativeTotalStatistics : INativeTotalStatistics
    {
        private const string _FOR_FOLDER_ERROR = "Failed to retrieve total native files count for folder: {folderId} and view: {viewId}.";
        private const string _FOR_PRODUCTION_ERROR = "Failed to retrieve total native files count for production set: {productionSetId}.";
        private const string _FOR_SAVED_SEARCH_ERROR = "Failed to retrieve total native files count for saved search id: {savedSearchId}.";

        private readonly IAPILog _logger;
        private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;

        public NativeTotalStatistics(IHelper helper, IRelativityObjectManagerFactory relativityObjectManagerFactory)
        {
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<NativeTotalStatistics>();
            _relativityObjectManagerFactory = relativityObjectManagerFactory;
        }

        public long ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
        {
            try
            {
                var queryBuilder = new DocumentQueryBuilder();
                QueryRequest query = queryBuilder.AddFolderCondition(folderId, viewId, includeSubFoldersTotals).AddHasNativeCondition().NoFields().Build();
                return CountQueryResults(query, workspaceArtifactId);
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
                var queryBuilder = new ProductionInformationQueryBuilder();
                QueryRequest query = queryBuilder.AddProductionSetCondition(productionSetId).AddHasNativeCondition().NoFields().Build();
                return CountQueryResults(query, workspaceArtifactId);
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
                var queryBuilder = new DocumentQueryBuilder();
                QueryRequest query = queryBuilder.AddSavedSearchCondition(savedSearchId).AddHasNativeCondition().NoFields().Build();
                return CountQueryResults(query, workspaceArtifactId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, _FOR_SAVED_SEARCH_ERROR, savedSearchId);
                throw;
            }
        }

        private long CountQueryResults(QueryRequest query, int workspaceArtifactId)
        {
            return _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId)
                .QueryTotalCount(query);
        }
    }
}