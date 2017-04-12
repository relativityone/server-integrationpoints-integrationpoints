using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
	public class DocumentTotalStatistics : IDocumentTotalStatistics
	{
		private const string _FOR_SAVED_SEARCH_ERROR = "Failed to retrieve total documents count for saved search id: {savedSearchId}.";
		private const string _FOR_FOLDER_ERROR = "Failed to retrieve total documents count for folder: {folderId} and view: {viewId}.";
		private const string _FOR_PRODUCTION_ERROR = "Failed to retrieve total documents count for production set: {productionSetId}.";

		private readonly IAPILog _logger;
		private readonly IRepositoryFactory _repositoryFactory;

		public DocumentTotalStatistics(IHelper helper, IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<DocumentTotalStatistics>();
		}

		public int ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			try
			{
				var query = new DocumentQueryBuilder().AddFolderCondition(folderId, viewId, includeSubFoldersTotals).NoFields().Build();
				return ExecuteQuery(query, workspaceArtifactId).TotalCount;
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
				var query = new ProductionInformationQueryBuilder().AddProductionSetCondition(productionSetId).NoFields().Build();
				return ExecuteQuery(query, workspaceArtifactId).TotalCount;
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
				var query = new DocumentQueryBuilder().AddSavedSearchCondition(savedSearchId).NoFields().Build();
				return ExecuteQuery(query, workspaceArtifactId).TotalCount;
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
	}
}