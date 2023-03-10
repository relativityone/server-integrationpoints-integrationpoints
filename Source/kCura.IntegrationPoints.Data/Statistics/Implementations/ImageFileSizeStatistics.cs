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
	public class ImageFileSizeStatistics : ImageStatisticsBase, IImageFileSizeStatistics
	{
		private const string _FOR_FOLDER_ERROR = "Failed to retrieve total image files size for folder: {folderId} and view: {viewId}.";
		private const string _FOR_PRODUCTION_ERROR = "Failed to retrieve total image files count for production set: {productionSetId}.";
		private const string _FOR_SAVED_SEARCH_ERROR = "Failed to retrieve total image files size for saved search id: {savedSearchId}.";
		private const string _PRODUCTION_DOCUMENT_FILE_TABLE_PREFIX = "ProductionDocumentFile_";
		protected readonly IChoiceService _choiceService;

		public ImageFileSizeStatistics(IHelper helper, IRelativityObjectManagerFactory relativityObjectManagerFactory, IChoiceService choiceService)
			: base(relativityObjectManagerFactory, helper, helper.GetLoggerFactory().GetLogger().ForContext<ImageTotalStatistics>())
		{
			_choiceService = choiceService;
		}

		public long ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			try
			{
				int choiceArtifactId = _choiceService.GetGuidOfYesChoiceOnHasImagesAsync(workspaceArtifactId).GetAwaiter().GetResult();
				var queryBuilder = new DocumentQueryBuilder();
				var query = queryBuilder.AddFolderCondition(folderId, viewId, includeSubFoldersTotals).AddHasImagesCondition(choiceArtifactId).NoFields().Build();
				var queryResult = ExecuteQuery(query, workspaceArtifactId);
				var artifactIds = queryResult.Select(x => x.ArtifactID).ToList();
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
				return GetTotalFileSize(productionSetId, workspaceArtifactId);
			}
			catch (Exception e)
			{
				_logger.LogError(e, _FOR_PRODUCTION_ERROR, productionSetId);
				return 0;
            }
		}

		public long ForSavedSearch(int workspaceArtifactId, int savedSearchId)
		{
			try
			{
				int choiceArtifactId = _choiceService.GetGuidOfYesChoiceOnHasImagesAsync(workspaceArtifactId).GetAwaiter().GetResult();
				var queryBuilder = new DocumentQueryBuilder();
				var query = queryBuilder.AddSavedSearchCondition(savedSearchId).AddHasImagesCondition(choiceArtifactId).NoFields().Build();
				var queryResult = ExecuteQuery(query, workspaceArtifactId);
				var artifactIds = queryResult.Select(x => x.ArtifactID).ToList();
				return GetTotalFileSize(artifactIds, workspaceArtifactId);
			}
			catch (Exception e)
			{
				_logger.LogError(e, _FOR_SAVED_SEARCH_ERROR, savedSearchId);
				return 0;
			}
		}

		public long GetTotalFileSize(IList<int> artifactIds, int workspaceArtifactId)
		{
			const string sqlText = "SELECT COALESCE(SUM([Size]),0) FROM [File] WHERE [Type] = @FileType AND [DocumentArtifactID] IN (SELECT * FROM @ArtifactIds)";

			DataTable idsDataTable = artifactIds.ToDataTable();

			SqlParameter artifactIdsParameter = new SqlParameter("@ArtifactIds", SqlDbType.Structured)
			{
				TypeName = "IDs",
				Value = idsDataTable
			};
			SqlParameter fileTypeParameter = new SqlParameter("@FileType", SqlDbType.Int)
			{
				Value = FileType.Tif
			};

			var dbContext = _helper.GetDBContext(workspaceArtifactId);
			return dbContext.ExecuteSqlStatementAsScalar<long>(sqlText, artifactIdsParameter, fileTypeParameter);
		}

		public long GetTotalFileSize(int productionSetId, int workspaceArtifactId)
		{
			const string sqlText = "SELECT COALESCE(SUM([Size]),0) FROM [{0}] AS PDF JOIN [File] AS F ON F.[FileID] = PDF.[ProducedFileID]";

			var tableName = $"{_PRODUCTION_DOCUMENT_FILE_TABLE_PREFIX}{productionSetId}";

			var dbContext = _helper.GetDBContext(workspaceArtifactId);
			return dbContext.ExecuteSqlStatementAsScalar<long>(string.Format(sqlText, tableName));
		}

		private List<RelativityObjectSlim> ExecuteQuery(QueryRequest query, int workspaceArtifactId)
		{
			using (var queryResult = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId)
				.QueryWithExportAsync(query, 0).GetAwaiter().GetResult())
			{
				return queryResult.GetAllResultsAsync().GetAwaiter().GetResult().ToList();
			}
		}
	}
}
