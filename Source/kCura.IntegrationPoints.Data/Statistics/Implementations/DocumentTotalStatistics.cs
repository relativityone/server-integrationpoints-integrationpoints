using System;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
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
		private readonly IRdoRepository _rdoRepository;

		public DocumentTotalStatistics(IHelper helper, IRdoRepository rdoRepository)
		{
			_rdoRepository = rdoRepository;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<DocumentTotalStatistics>();
		}

		public long ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			try
			{
				Query<RDO> query = new DocumentQueryBuilder().AddFolderCondition(folderId, viewId, includeSubFoldersTotals).NoFields().Build();
				return ExecuteQuery(query).TotalCount;
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
				Query<RDO> query = new ProductionInformationQueryBuilder().AddProductionSetCondition(productionSetId).NoFields().Build();
				return ExecuteQuery(query).TotalCount;
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
				Query<RDO> query = new DocumentQueryBuilder().AddSavedSearchCondition(savedSearchId).NoFields().Build();
				return ExecuteQuery(query).TotalCount;
			}
			catch (Exception e)
			{
				_logger.LogError(e, _FOR_SAVED_SEARCH_ERROR, savedSearchId);
				throw;
			}
		}

		private QueryResultSet<RDO> ExecuteQuery(Query<RDO> query)
		{
			return _rdoRepository.Query(query);
		}
	}
}