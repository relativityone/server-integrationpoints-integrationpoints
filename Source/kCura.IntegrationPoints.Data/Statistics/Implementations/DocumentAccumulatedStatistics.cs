using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
	public class DocumentAccumulatedStatistics : IDocumentAccumulatedStatistics
	{
		private readonly IRelativityObjectManagerFactory _relativityObjectManagerFactory;
		private readonly INativeFileSizeStatistics _nativeFileSizeStatistics;
		private readonly IImageFileSizeStatistics _imageFileSizeStatistics;
		private readonly IAPILog _logger;

		public DocumentAccumulatedStatistics(
			IRelativityObjectManagerFactory relativityObjectManagerFactory,
			INativeFileSizeStatistics nativeFileSizeStatistics,
			IImageFileSizeStatistics imageFileSizeStatistics,
			IAPILog logger)
		{
			_relativityObjectManagerFactory = relativityObjectManagerFactory;
			_nativeFileSizeStatistics = nativeFileSizeStatistics;
			_imageFileSizeStatistics = imageFileSizeStatistics;
			_logger = logger;
		}

		public Task<DocumentsStatistics> GetNativesStatisticsForSavedSearchAsync(int workspaceId, int savedSearchId, bool calculateSize)
		{
			try
			{
				DocumentsStatistics statistics = new DocumentsStatistics();

				QueryRequest query = new DocumentQueryBuilder()
					.AddSavedSearchCondition(savedSearchId)
					.AddField(DocumentFieldsConstants.HasNativeFieldGuid)
					.Build();

				List<RelativityObject> documents = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceId).Query(query);

				statistics.DocumentsCount = documents.Count;
				statistics.TotalNativesCount = documents.Count(x => (bool)x[DocumentFieldsConstants.HasNativeFieldGuid].Value == true);
				if (calculateSize)
				{
					statistics.TotalNativesSizeBytes = _nativeFileSizeStatistics.GetTotalFileSize(documents.Select(x => x.ArtifactID), workspaceId);
				}

				return Task.FromResult(statistics);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception occurred while calculating natives statistics for Saved Search ID: {savedSearchId} in Workspace ID: {workspaceId}", savedSearchId, workspaceId);
				throw;
			}
		}

		public Task<DocumentsStatistics> GetImagesStatisticsForSavedSearchAsync(int workspaceId, int savedSearchId, bool calculateSize)
		{
			try
			{
				DocumentsStatistics statistics = new DocumentsStatistics();

				QueryRequest query = new DocumentQueryBuilder()
					.AddSavedSearchCondition(savedSearchId)
					.AddField(DocumentFieldsConstants.HasImagesFieldName)
					.AddField(DocumentFieldsConstants.RelativityImageCountGuid)
					.Build();

				List<RelativityObject> documents = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceId).Query(query);

				statistics.DocumentsCount = documents.Count;
				statistics.TotalImagesCount = documents.Sum(x => Convert.ToInt64(x[DocumentFieldsConstants.RelativityImageCountGuid].Value ?? 0));

				if (calculateSize)
				{
					List<RelativityObject> documentsWithImages = documents.Where(x =>
					{
						FieldValuePair hasImagesFieldValuePair = x[DocumentFieldsConstants.HasImagesFieldName];
						Choice choice = (Choice)hasImagesFieldValuePair.Value;
						return choice.Name == DocumentFieldsConstants.HasImagesYesChoiceName;
					}).ToList();
					statistics.TotalImagesSizeBytes = _imageFileSizeStatistics.GetTotalFileSize(documentsWithImages.Select(x => x.ArtifactID).ToList(), workspaceId);
				}

				return Task.FromResult(statistics);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception occurred while calculating images statistics for Saved Search ID: {savedSearchId} in Workspace ID: {workspaceId}", savedSearchId, workspaceId);
				throw;
			}
		}

		public Task<DocumentsStatistics> GetImagesStatisticsForProductionAsync(int workspaceId, int productionId, bool calculateSize)
		{
			try
			{
				DocumentsStatistics statistics = new DocumentsStatistics();

				QueryRequest query = new ProductionInformationQueryBuilder()
					.AddProductionSetCondition(productionId)
					.AddField(ProductionConsts.ImageCountFieldGuid)
					.Build();

				List<RelativityObject> documents = _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceId).Query(query);

				statistics.DocumentsCount = documents.Count; 
				statistics.TotalImagesCount = documents.Sum(x => Convert.ToInt64(x[ProductionConsts.ImageCountFieldGuid].Value ?? 0));
				if (calculateSize)
				{
					statistics.TotalImagesSizeBytes = _imageFileSizeStatistics.GetTotalFileSize(productionId, workspaceId);
				}

				return Task.FromResult(statistics);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception occurred while calculating images statistics for Production ID: {productionId} in Workspace ID: {workspaceId}", productionId, workspaceId);
				throw;
			}
		}
	}
}