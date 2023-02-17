using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using Relativity;
using Relativity.API;
using Relativity.Services.Field;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
    public class NativeFileSizeStatistics : INativeFileSizeStatistics
    {
        private const string _FOR_FOLDER_ERROR = "Failed to retrieve total native files size for folder: {folderId} and view: {viewId}.";
        private const string _FOR_PRODUCTION_ERROR = "Failed to retrieve total native files count for production set: {productionSetId}.";
        private const string _FOR_SAVED_SEARCH_ERROR = "Failed to retrieve total native files size for saved search id: {savedSearchId}.";
        private readonly IHelper _helper;
        private readonly IAPILog _logger;
        private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;
        private readonly IDbContextFactory _dbContextFactory;

        public NativeFileSizeStatistics(IHelper helper, IRelativityObjectManagerFactory relativityObjectManagerFactory)
        {
            _helper = helper;
            _logger = _helper.GetLoggerFactory().GetLogger().ForContext<NativeFileSizeStatistics>();
            _relativityObjectManagerFactory = relativityObjectManagerFactory;
            _dbContextFactory = new DbContextFactory(_helper, _logger);
        }

        public long ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
        {
            try
            {
                var queryBuilder = new DocumentQueryBuilder();
                QueryRequest query = queryBuilder.AddFolderCondition(folderId, viewId, includeSubFoldersTotals).AddHasNativeCondition().NoFields().Build();
                List<RelativityObjectSlim> queryResult = ExecuteQuery(query, workspaceArtifactId);
                IEnumerable<int> artifactIds = GetArtifactIds(queryResult);
                return GetTotalFileSize(artifactIds, workspaceArtifactId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, _FOR_FOLDER_ERROR, folderId, viewId);
                return 0;
            }
        }

        public long ForProduction(int workspaceArtifactId, int productionSetId)
        {
            try
            {
                var queryBuilder = new ProductionInformationQueryBuilder();
                QueryRequest query = queryBuilder.AddProductionSetCondition(productionSetId).AddHasNativeCondition().AddField(ProductionConsts.DocumentFieldGuid).Build();
                List<RelativityObjectSlim> queryResult = ExecuteQuery(query, workspaceArtifactId, out var fieldsMetadata);
                IEnumerable<int> artifactIds = queryResult.Select(RetrieveArtifactIdFromFieldValue(ProductionConsts.DocumentFieldGuid, fieldsMetadata));
                return GetTotalFileSize(artifactIds, workspaceArtifactId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, _FOR_PRODUCTION_ERROR, productionSetId);
                return 0;
            }
        }

        private Func<RelativityObjectSlim, int> RetrieveArtifactIdFromFieldValue(Guid fieldGuid,
            List<FieldMetadata> fieldsMetadata)
        {
            int index = fieldsMetadata.FindIndex(field => field.Guids.Contains(fieldGuid));
            return relativityObject =>
            {
                // field values are of type JObject from Newtonsoft, which has dynamic view
                dynamic value = (dynamic)relativityObject.Values.ElementAtOrDefault(index);

                return (int)value.ArtifactID;
            };
        }

        public long ForSavedSearch(int workspaceArtifactId, int savedSearchId)
        {
            try
            {
                var queryBuilder = new DocumentQueryBuilder();
                QueryRequest query = queryBuilder.AddSavedSearchCondition(savedSearchId).AddHasNativeCondition().NoFields().Build();
                List<RelativityObjectSlim> queryResult = ExecuteQuery(query, workspaceArtifactId);
                IEnumerable<int> artifactIds = GetArtifactIds(queryResult);
                return GetTotalFileSize(artifactIds, workspaceArtifactId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, _FOR_SAVED_SEARCH_ERROR, savedSearchId);
                return 0;
            }
        }

        public long GetTotalFileSize(IEnumerable<int> artifactIds, int workspaceArtifactId)
        {
            const string sqlText = "SELECT COALESCE(SUM([Size]),0) FROM [File] WHERE [Type] = @FileType AND [DocumentArtifactID] IN (SELECT * FROM @ArtifactIds)";

            IEnumerable<SqlParameter> sqlParams = new[]
            {
                new SqlParameter("@ArtifactIds", SqlDbType.Structured)
                {
                    TypeName = "IDs",
                    Value = artifactIds.ToDataTable()
                },
                new SqlParameter("@FileType", SqlDbType.Int)
                {
                    Value = FileType.Native
                }
            };

            IWorkspaceDBContext dbContext = _dbContextFactory.CreateWorkspaceDbContext(workspaceArtifactId);
            return dbContext.ExecuteSqlStatementAsScalar<long>(sqlText, sqlParams);
        }

        private List<RelativityObjectSlim> ExecuteQuery(QueryRequest query, int workspaceArtifactId, out List<FieldMetadata> fieldsMetadata)
        {
            using (var queryResult = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId)
                .QueryWithExportAsync(query, 0).GetAwaiter().GetResult())
            {
                fieldsMetadata = queryResult.ExportResult.FieldData;
                return queryResult.GetAllResultsAsync().GetAwaiter().GetResult().ToList();
            }
        }

        private List<RelativityObjectSlim> ExecuteQuery(QueryRequest query, int workspaceArtifactId) =>
            ExecuteQuery(query, workspaceArtifactId, out _);
        private IEnumerable<int> GetArtifactIds(IEnumerable<RelativityObjectSlim> relativityObjects) =>
            relativityObjects.Select(GetArtifactId);
        private int GetArtifactId(RelativityObjectSlim relativityObject) => relativityObject.ArtifactID;

    }
}
