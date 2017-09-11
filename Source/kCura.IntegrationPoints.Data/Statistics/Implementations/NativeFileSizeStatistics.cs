using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.Relativity.Client.DTOs;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
	public class NativeFileSizeStatistics : INativeFileSizeStatistics
	{
		private const string _FOR_FOLDER_ERROR = "Failed to retrieve total native files size for folder: {folderId} and view: {viewId}.";
		private const string _FOR_PRODUCTION_ERROR = "Failed to retrieve total native files count for production set: {productionSetId}.";
		private const string _FOR_SAVED_SEARCH_ERROR = "Failed to retrieve total native files size for saved search id: {savedSearchId}.";

		private readonly IHelper _helper;
		private readonly IAPILog _logger;
		private readonly IRepositoryFactory _repositoryFactory;

		public NativeFileSizeStatistics(IHelper helper, IRepositoryFactory repositoryFactory)
		{
			_helper = helper;
			_logger = _helper.GetLoggerFactory().GetLogger().ForContext<NativeFileSizeStatistics>();
			_repositoryFactory = repositoryFactory;
		}

		public int ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			try
			{
				var queryBuilder = new DocumentQueryBuilder();
				Query<RDO> query = queryBuilder.AddFolderCondition(folderId, viewId, includeSubFoldersTotals).AddHasNativeCondition().NoFields().Build();
				QueryResultSet<RDO> queryResult = ExecuteQuery(query, workspaceArtifactId);
				List<int> artifactIds = queryResult.Results.Select(x => x.Artifact.ArtifactID).ToList();
				return GetTotalFileSize(artifactIds, workspaceArtifactId);
			}
			catch (Exception e)
			{
				_logger.LogError(e, _FOR_FOLDER_ERROR, folderId, viewId);
				throw;
			}
		}
		
		public int ForProduction(int workspaceArtifactId, int productionSetId)
		{
			try
			{
				var queryBuilder = new ProductionInformationQueryBuilder();
				Query<RDO> query = queryBuilder.AddProductionSetCondition(productionSetId).AddHasNativeCondition().AddField(ProductionConsts.DocumentFieldGuid).Build();
				QueryResultSet<RDO> queryResult = ExecuteQuery(query, workspaceArtifactId);
				List<int> artifactIds = queryResult.Results.Select(x => ((Artifact) x.Artifact[ProductionConsts.DocumentFieldGuid].Value).ArtifactID).ToList();
				return GetTotalFileSize(artifactIds, workspaceArtifactId);
			}
			catch (Exception e)
			{
				_logger.LogError(e, _FOR_PRODUCTION_ERROR, productionSetId);
				throw;
			}
		}

		public int ForSavedSearch(int workspaceArtifactId, int savedSearchId)
		{
			try
			{
				var queryBuilder = new DocumentQueryBuilder();
				Query<RDO> query = queryBuilder.AddSavedSearchCondition(savedSearchId).AddHasNativeCondition().NoFields().Build();
				QueryResultSet<RDO> queryResult = ExecuteQuery(query, workspaceArtifactId);
				List<int> artifactIds = queryResult.Results.Select(x => x.Artifact.ArtifactID).ToList();
				return GetTotalFileSize(artifactIds, workspaceArtifactId);
			}
			catch (Exception e)
			{
				_logger.LogError(e, _FOR_SAVED_SEARCH_ERROR, savedSearchId);
				throw;
			}
		}

		private QueryResultSet<RDO> ExecuteQuery(Query<RDO> query, int workspaceArtifactId)
		{
			return _repositoryFactory.GetRdoRepository(workspaceArtifactId).Query(query);
		}

		private int GetTotalFileSize(IList<int> artifactIds, int workspaceArtifactId)
		{
			const string sqlText = "SELECT COALESCE(SUM([Size]),0) FROM [File] WHERE [Type] = @FileType AND [DocumentArtifactID] IN (SELECT * FROM @ArtifactIds)";
			
			var artifactIdsParameter = new SqlParameter("@ArtifactIds", SqlDbType.Structured)
			{
				TypeName = "IDs",
				Value = artifactIds.ToDataTable()
			};

			var fileTypeParameter = new SqlParameter("@FileType", SqlDbType.Int)
			{
				Value = FileType.Native
			};

			IDBContext dbContext = _helper.GetDBContext(workspaceArtifactId);
			return dbContext.ExecuteSqlStatementAsScalar<int>(sqlText, artifactIdsParameter, fileTypeParameter);
		}
		
	}
}