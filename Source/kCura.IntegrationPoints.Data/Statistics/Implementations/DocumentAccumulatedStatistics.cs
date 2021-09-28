using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
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

        public async Task<DocumentsStatistics> GetNativesStatisticsForSavedSearchAsync(int workspaceId,
            int savedSearchId, CancellationToken token = default(CancellationToken))
        {
            bool DocumentHasNative(RelativityObjectSlim document) =>
                (document.Values.FirstOrDefault() as bool?) == true;

            try
            {
                DocumentsStatistics statistics = new DocumentsStatistics();

                QueryRequest query = new DocumentQueryBuilder()
                    .AddSavedSearchCondition(savedSearchId)
                    .AddField(DocumentFieldsConstants.HasNativeFieldGuid)
                    .Build();

                RelativityObjectSlim[] documents = await QueryDocumentsWithExport(workspaceId, query, token);

                statistics.DocumentsCount = documents.Length;
                statistics.TotalNativesCount = documents.Count(DocumentHasNative);
                statistics.TotalNativesSizeBytes =
                    _nativeFileSizeStatistics.GetTotalFileSize(documents.Select(x => x.ArtifactID), workspaceId);

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception occurred while calculating natives statistics for Saved Search ID: {savedSearchId} in Workspace ID: {workspaceId}",
                    savedSearchId, workspaceId);
                throw;
            }
        }

        public async Task<DocumentsStatistics> GetImagesStatisticsForSavedSearchAsync(int workspaceId,
            int savedSearchId, bool calculateSize, CancellationToken token = default(CancellationToken))
        {
            long GetDocumentImageCount(RelativityObjectSlim document, int imageCountFieldIndex) =>
                Convert.ToInt64(document.Values[imageCountFieldIndex] ?? 0);


            try
            {
                DocumentsStatistics statistics = new DocumentsStatistics();

                QueryRequest query = new DocumentQueryBuilder()
                    .AddSavedSearchCondition(savedSearchId)
                    .AddField(DocumentFieldsConstants.HasImagesFieldName)
                    .AddField(DocumentFieldsConstants.RelativityImageCountGuid)
                    .Build();

                RelativityObjectSlim[] documents =
                    await QueryDocumentsWithExport(workspaceId, query, token).ConfigureAwait(false);

                statistics.DocumentsCount = documents.Length;
                statistics.TotalImagesCount = documents.Sum(x => GetDocumentImageCount(x, 1));

                if (calculateSize)
                {
                    List<int> documentsWithImagesArtifactIDs = documents.Where(x =>
                    {
                        Choice choice = (Choice)x.Values[0];
                        return choice.Name == DocumentFieldsConstants.HasImagesYesChoiceName;
                    }).Select(x => x.ArtifactID).ToList();
                    statistics.TotalImagesSizeBytes =
                        _imageFileSizeStatistics.GetTotalFileSize(documentsWithImagesArtifactIDs, workspaceId);
                }

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception occurred while calculating images statistics for Saved Search ID: {savedSearchId} in Workspace ID: {workspaceId}",
                    savedSearchId, workspaceId);
                throw;
            }
        }

        public async Task<DocumentsStatistics> GetImagesStatisticsForProductionAsync(int workspaceId, int productionId,
            CancellationToken token = default(CancellationToken))
        {
            long DocumentHasImage(RelativityObjectSlim document) =>
                Convert.ToInt64(document.Values.FirstOrDefault() ?? 0);

            try
            {
                DocumentsStatistics statistics = new DocumentsStatistics();

                QueryRequest query = new ProductionInformationQueryBuilder()
                    .AddProductionSetCondition(productionId)
                    .AddField(ProductionConsts.ImageCountFieldGuid)
                    .Build();

                RelativityObjectSlim[] documents =
                    await QueryDocumentsWithExport(workspaceId, query, token).ConfigureAwait(false);

                statistics.DocumentsCount = documents.Length;
                statistics.TotalImagesCount = documents.Sum(x => DocumentHasImage(x));
                statistics.TotalImagesSizeBytes = _imageFileSizeStatistics.GetTotalFileSize(productionId, workspaceId);

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception occurred while calculating images statistics for Production ID: {productionId} in Workspace ID: {workspaceId}",
                    productionId, workspaceId);
                throw;
            }
        }

        private async Task<RelativityObjectSlim[]> QueryDocumentsWithExport(int workspaceId, QueryRequest query,
            CancellationToken token)
        {
            IRelativityObjectManager objectManager =
                _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceId);

            using (IExportQueryResult export = await objectManager.QueryWithExportAsync(query, 0).ConfigureAwait(false))
            {
                IEnumerable<RelativityObjectSlim> documents =
                    await export.GetAllResultsAsync(token).ConfigureAwait(false);
                
                return documents.ToArray();
            }
        }
    }
}