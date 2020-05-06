using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using Relativity.API;
using Relativity.Data.Cache.Field;
using Relativity.Services.Objects.DataContracts;
using FieldMetadata = Relativity.Services.Field.FieldMetadata;

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
				var queryResult = ExecuteQuery(query, workspaceArtifactId, out var fieldsMetadata);
				return SumDocumentImages(queryResult, fieldsMetadata);
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
				var queryResult = ExecuteQuery(query, workspaceArtifactId, out var fieldsMetadata);
				return SumProductionImages(queryResult, fieldsMetadata);
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
				var queryResult = ExecuteQuery(query, workspaceArtifactId, out var fieldsMetadata);
				return SumDocumentImages(queryResult, fieldsMetadata);
			}
			catch (Exception e)
			{
				_logger.LogError(e, _FOR_SAVED_SEARCH_ERROR, savedSearchId);
				throw;
			}
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

		private long SumDocumentImages(List<RelativityObjectSlim> documents, List<FieldMetadata> fieldsMetadata)
		{
			return SumFieldValues(documents, fieldsMetadata, DocumentFieldsConstants.RelativityImageCount);
		}

		private long SumProductionImages(List<RelativityObjectSlim> documents, List<FieldMetadata> fieldsMetadata)
		{
			return SumFieldValues(documents, fieldsMetadata, ProductionConsts.ImageCountFieldGuid);
		}

		private long SumFieldValues(List<RelativityObjectSlim> documents, List<FieldMetadata> fieldsMetadata, Guid fieldGuid)
		{
			int index = fieldsMetadata.FindIndex(x => x.Guids.Contains(fieldGuid));
			return documents.Select(GetFunctionForRetrieveFieldValue(index)).Select(ConvertObjectToLong).Sum();
		}

		private Func<RelativityObjectSlim, object> GetFunctionForRetrieveFieldValue(int index)
		{
			return relativityObject =>
			{
				return relativityObject.Values.ElementAtOrDefault(index);
			};
		}

		private long ConvertObjectToLong(object obj)
		{
			return Convert.ToInt64(obj ?? 0);
		}
	}
}