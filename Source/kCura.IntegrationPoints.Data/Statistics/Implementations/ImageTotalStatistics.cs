using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
	public class ImageTotalStatistics : IImageTotalStatistics
	{
		private const string _FOR_PRODUCTION_ERROR =
			"Failed to retrieve total images count for production set: {productionSetId}.";

		private const string _FOR_FOLDER_ERROR =
			"Failed to retrieve total images count for folder: {folderId} and view: {viewId}.";

		private const string _FOR_SAVED_SEARCH_ERROR =
			"Failed to retrieve total images count for saved search id: {savedSearchId}.";

		private readonly IAPILog _logger;
		private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;

		public ImageTotalStatistics(IHelper helper, IRelativityObjectManagerFactory relativityObjectManagerFactory)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ImageTotalStatistics>();
			_relativityObjectManagerFactory = relativityObjectManagerFactory;
		}

		public long ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			try
			{
				var queryBuilder = new DocumentQueryBuilder();
				var query = queryBuilder.AddFolderCondition(folderId, viewId, includeSubFoldersTotals).AddHasImagesCondition()
					.AddField(DocumentFieldsConstants.RelativityImageCount).Build();
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
				var query = queryBuilder.AddProductionSetCondition(productionSetId).AddField(ProductionConsts.ImageCountFieldGuid)
					.Build();
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
				var query = queryBuilder.AddSavedSearchCondition(savedSearchId).AddHasImagesCondition()
					.AddField(DocumentFieldsConstants.RelativityImageCount).Build();
				var queryResult = ExecuteQuery(query, workspaceArtifactId);
				return SumDocumentImages(queryResult);
			}
			catch (Exception e)
			{
				_logger.LogError(e, _FOR_SAVED_SEARCH_ERROR, savedSearchId);
				throw;
			}
		}

		private List<RelativityObject> ExecuteQuery(QueryRequest query, int workspaceArtifactId)
		{
			return _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId).Query(query);
		}

		private long SumDocumentImages(List<RelativityObject> documents)
		{
			return SumFieldValues(documents, DocumentFieldsConstants.RelativityImageCount);
		}

		private long SumProductionImages(List<RelativityObject> documents)
		{
			return SumFieldValues(documents, ProductionConsts.ImageCountFieldGuid);
		}

		private long SumFieldValues(List<RelativityObject> documents, Guid fieldGuid)
		{
			return documents.Select(GetFunctionForRetrieveFieldValue(fieldGuid)).Select(ConvertObjectToLong).Sum();
		}

		private Func<RelativityObject, object> GetFunctionForRetrieveFieldValue(Guid fieldGuid)
		{
			return relativityObject => relativityObject.FieldValues.FirstOrDefault(field => field.Field.Guids.Contains(fieldGuid))?.Value;
		}

		private long ConvertObjectToLong(object obj)
		{
			return Convert.ToInt64(obj ?? 0);
		}
	}
}