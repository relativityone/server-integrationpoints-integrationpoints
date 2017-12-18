using System;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
	public class ImageTotalStatistics : IImageTotalStatistics
	{
		private const string _FOR_PRODUCTION_ERROR = "Failed to retrieve total images count for production set: {productionSetId}.";
		private const string _FOR_FOLDER_ERROR = "Failed to retrieve total images count for folder: {folderId} and view: {viewId}.";
		private const string _FOR_SAVED_SEARCH_ERROR = "Failed to retrieve total images count for saved search id: {savedSearchId}.";

		private readonly IAPILog _logger;
		private readonly IRepositoryFactory _repositoryFactory;

		public ImageTotalStatistics(IHelper helper, IRepositoryFactory repositoryFactory)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ImageTotalStatistics>();
			_repositoryFactory = repositoryFactory;
		}

		public long ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			try
			{
				var queryBuilder = new DocumentQueryBuilder();
				var query = queryBuilder.AddFolderCondition(folderId, viewId, includeSubFoldersTotals).AddHasImagesCondition().AddField(DocumentFieldsConstants.RelativityImageCount).Build();
				var queryResult = ExecuteQuery(query, workspaceArtifactId);
				return SumDocumentImages(queryResult);
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
				var query = queryBuilder.AddProductionSetCondition(productionSetId).AddField(ProductionConsts.ImageCountFieldGuid).Build();
				var queryResult = ExecuteQuery(query, workspaceArtifactId);
				return SumProductionImages(queryResult);
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
				var query = queryBuilder.AddSavedSearchCondition(savedSearchId).AddHasImagesCondition().AddField(DocumentFieldsConstants.RelativityImageCount).Build();
				var queryResult = ExecuteQuery(query, workspaceArtifactId);
				return SumDocumentImages(queryResult);
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

		private long SumDocumentImages(QueryResultSet<RDO> documents)
		{
			return documents.Results.Sum(x => x.Artifact[DocumentFieldsConstants.RelativityImageCount].ValueAsWholeNumber ?? 0L);
		}

		private long SumProductionImages(QueryResultSet<RDO> documents)
		{
			return documents.Results.Sum(x => x.Artifact[ProductionConsts.ImageCountFieldGuid].ValueAsWholeNumber ?? 0L);
		}
	}
}