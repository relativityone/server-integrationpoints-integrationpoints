using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
	public class NativeTotalStatistics : INativeTotalStatistics
	{
		private const string _FOR_FOLDER_ERROR = "Failed to retrieve total native files count for folder: {folderId} and view: {viewId}.";
		private const string _FOR_PRODUCTION_ERROR = "Failed to retrieve total native files count for production set: {productionSetId}.";
		private const string _FOR_SAVED_SEARCH_ERROR = "Failed to retrieve total native files count for saved search id: {savedSearchId}.";

		private readonly IAPILog _logger;
		private readonly IRepositoryFactory _repositoryFactory;

		public NativeTotalStatistics(IHelper helper, IRepositoryFactory repositoryFactory)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<NativeTotalStatistics>();
			_repositoryFactory = repositoryFactory;
		}

		public int ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			try
			{
				var queryBuilder = new DocumentQueryBuilder();
				var query = queryBuilder.AddFolderCondition(folderId, viewId, includeSubFoldersTotals).AddHasNativeCondition().NoFields().Build();
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
				var queryBuilder = new ProductionInformationQueryBuilder();
				var query = queryBuilder.AddProductionSetCondition(productionSetId).AddHasNativeCondition().NoFields().Build();
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
				var queryBuilder = new DocumentQueryBuilder();
				var query = queryBuilder.AddSavedSearchCondition(savedSearchId).AddHasNativeCondition().NoFields().Build();
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