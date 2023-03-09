using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using Relativity.API;
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
		private readonly IChoiceService _choiceService;

		public ImageTotalStatistics(IHelper helper, IRelativityObjectManagerFactory relativityObjectManagerFactory, IChoiceService choiceService)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ImageTotalStatistics>();
			_relativityObjectManagerFactory = relativityObjectManagerFactory;
			_choiceService = choiceService;
		}

		public long ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals)
		{
			try
			{
				int choiceArtifactId = _choiceService.GetGuidOfYesChoiceOnHasImagesAsync(workspaceArtifactId).GetAwaiter().GetResult();
				var queryBuilder = new DocumentQueryBuilder();
				QueryRequest query = queryBuilder.AddFolderCondition(folderId, viewId, includeSubFoldersTotals).AddHasImagesCondition(choiceArtifactId)
					.AddField(DocumentFieldsConstants.RelativityImageCount).Build();
				long sum = ExecuteQuery(query, workspaceArtifactId, SumDocumentImages);
				return sum;
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
				QueryRequest query = queryBuilder.AddProductionSetCondition(productionSetId).AddField(ProductionConsts.ImageCountFieldGuid)
					.Build();
				long sum = ExecuteQuery(query, workspaceArtifactId, SumProductionImages);
				return sum;
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
				int choiceArtifactId = _choiceService.GetGuidOfYesChoiceOnHasImagesAsync(workspaceArtifactId).GetAwaiter().GetResult();
				var queryBuilder = new DocumentQueryBuilder();
				QueryRequest query = queryBuilder.AddSavedSearchCondition(savedSearchId).AddHasImagesCondition(choiceArtifactId)
					.AddField(DocumentFieldsConstants.RelativityImageCount).Build();
				long sum = ExecuteQuery(query, workspaceArtifactId, SumDocumentImages);
				return sum;
			}
			catch (Exception e)
			{
				_logger.LogError(e, _FOR_SAVED_SEARCH_ERROR, savedSearchId);
				throw;
			}
		}

		private long ExecuteQuery(QueryRequest query, int workspaceArtifactId, Func<List<RelativityObjectSlim>, List<FieldMetadata>, long> getValueFromChunkFunction)
		{
			using (var queryResult = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId)
				.QueryWithExportAsync(query, 0).GetAwaiter().GetResult())
			{
				int startIndex = 0;
				List<RelativityObjectSlim> result;

				long sum = 0;
				do
				{
					result = queryResult.GetNextBlockAsync(startIndex).GetAwaiter().GetResult().ToList();
					if (result.Any())
					{
						sum += getValueFromChunkFunction(result, queryResult.ExportResult.FieldData);
					}

					startIndex += result.Count;
				}
				while (result.Any());

				return sum;
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

		private long SumFieldValues(List<RelativityObjectSlim> documentsChunk, List<FieldMetadata> fieldsMetadata, Guid fieldGuid)
		{
			int index = fieldsMetadata.FindIndex(x => x.Guids.Contains(fieldGuid));

			return documentsChunk.Select(GetFunctionForRetrieveFieldValue(index)).Select(ConvertObjectToLong).Sum();
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
