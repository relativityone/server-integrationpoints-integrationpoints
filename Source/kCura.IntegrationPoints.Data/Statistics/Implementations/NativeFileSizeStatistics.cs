﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using Relativity;
using Relativity.API;
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

		public NativeFileSizeStatistics(IHelper helper, IRelativityObjectManagerFactory relativityObjectManagerFactory)
		{
			_helper = helper;
			_logger = _helper.GetLoggerFactory().GetLogger().ForContext<NativeFileSizeStatistics>();
			_relativityObjectManagerFactory = relativityObjectManagerFactory;
		}

		public long ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			try
			{
				var queryBuilder = new DocumentQueryBuilder();
				QueryRequest query = queryBuilder.AddFolderCondition(folderId, viewId, includeSubFoldersTotals).AddHasNativeCondition().NoFields().Build();
				List<RelativityObject> queryResult = ExecuteQuery(query, workspaceArtifactId);
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
				List<RelativityObject> queryResult = ExecuteQuery(query, workspaceArtifactId);
				IEnumerable<int> artifactIds = queryResult.Select(
						x => (RelativityObjectValue)GetFunctionForRetrieveFieldValue(ProductionConsts.DocumentFieldGuid)(x))
					.Select(x => x.ArtifactID);
				return GetTotalFileSize(artifactIds, workspaceArtifactId);
			}
			catch (Exception e)
			{
				_logger.LogError(e, _FOR_PRODUCTION_ERROR, productionSetId);
				return 0;
			}
		}

		private Func<RelativityObject, object> GetFunctionForRetrieveFieldValue(Guid fieldGuid)
		{
			return relativityObject => relativityObject.FieldValues.FirstOrDefault(field => field.Field.Guids.Contains(fieldGuid))?.Value;
		}

		public long ForSavedSearch(int workspaceArtifactId, int savedSearchId)
		{
			try
			{
				var queryBuilder = new DocumentQueryBuilder();
				QueryRequest query = queryBuilder.AddSavedSearchCondition(savedSearchId).AddHasNativeCondition().NoFields().Build();
				List<RelativityObject> queryResult = ExecuteQuery(query, workspaceArtifactId);
				IEnumerable<int> artifactIds = GetArtifactIds(queryResult);
				return GetTotalFileSize(artifactIds, workspaceArtifactId);
			}
			catch (Exception e)
			{
				_logger.LogError(e, _FOR_SAVED_SEARCH_ERROR, savedSearchId);
				return 0;
			}
		}

		private List<RelativityObject> ExecuteQuery(QueryRequest query, int workspaceArtifactId)
		{
			return _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId).Query(query);
		}

		private IEnumerable<int> GetArtifactIds(IEnumerable<RelativityObject> relativityObjects) =>
			relativityObjects.Select(GetArtifactId);

		private int GetArtifactId(RelativityObject relativityObject) => relativityObject.ArtifactID;

		private long GetTotalFileSize(IEnumerable<int> artifactIds, int workspaceArtifactId)
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
			return dbContext.ExecuteSqlStatementAsScalar<long>(sqlText, artifactIdsParameter, fileTypeParameter);
		}

	}
}