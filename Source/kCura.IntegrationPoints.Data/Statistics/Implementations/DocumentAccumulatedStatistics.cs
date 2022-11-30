using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public async Task<DocumentsStatistics> GetNativesStatisticsForSavedSearchAsync(
            int workspaceId,
            int savedSearchId,
            CancellationToken token = default(CancellationToken))
        {
            DocumentsStatistics StatisticsCalculation(DocumentsStatistics statistics, IEnumerable<RelativityObjectSlim> result)
            {
                bool DocumentHasNative(RelativityObjectSlim document) =>
                    (document.Values.FirstOrDefault() as bool?) == true;

                statistics.DocumentsCount += result.Count();
                statistics.TotalNativesCount += result.Count(DocumentHasNative);

                statistics.TotalNativesSizeBytes +=
                    _nativeFileSizeStatistics.GetTotalFileSize(result.Select(x => x.ArtifactID), workspaceId);

                return statistics;
            }

            try
            {
                QueryRequest query = new DocumentQueryBuilder()
                    .AddSavedSearchCondition(savedSearchId)
                    .AddField(DocumentFieldsConstants.HasNativeFieldGuid)
                    .Build();

                DocumentsStatistics statistics = await QueryDocumentsWithExport(workspaceId, query,
                    nameof(GetNativesStatisticsForSavedSearchAsync), token,
                    StatisticsCalculation).ConfigureAwait(false);

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
            DocumentsStatistics StatisticsCalculation(DocumentsStatistics stats, IEnumerable<RelativityObjectSlim> result)
            {
                long GetDocumentImageCount(RelativityObjectSlim document, int imageCountFieldIndex) => Convert.ToInt64(document.Values[imageCountFieldIndex] ?? 0);

                stats.DocumentsCount += result.Count();
                stats.TotalImagesCount += result.Sum(x => GetDocumentImageCount(x, 1));

                if (calculateSize)
                {
                    List<int> documentsWithImagesArtifactIDs = result.Where(x =>
                    {
                        dynamic choice = x.Values[0];
                        return choice.Name == DocumentFieldsConstants.HasImagesYesChoiceName;
                    })
                    .Select(x => x.ArtifactID)
                    .ToList();

                    stats.TotalImagesSizeBytes += _imageFileSizeStatistics.GetTotalFileSize(documentsWithImagesArtifactIDs, workspaceId);
                }

                return stats;
            }

            try
            {

                // TODO: REMOVE AFTER TESTS! IF YOU CAN SEE IT DURING REVIEW => SCREAM();
                //await Task.Delay(TimeSpan.FromSeconds(40)).ConfigureAwait(false);

                QueryRequest query = new DocumentQueryBuilder()
                    .AddSavedSearchCondition(savedSearchId)
                    .AddField(DocumentFieldsConstants.HasImagesFieldName)
                    .AddField(DocumentFieldsConstants.RelativityImageCountGuid)
                    .Build();

                DocumentsStatistics statistics = await QueryDocumentsWithExport(workspaceId, query,
                    nameof(GetImagesStatisticsForSavedSearchAsync), token,
                    StatisticsCalculation).ConfigureAwait(false);

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
            DocumentsStatistics StatisticsCalculation(DocumentsStatistics statistics, IEnumerable<RelativityObjectSlim> result)
            {
                long DocumentHasImage(RelativityObjectSlim document) =>
                    Convert.ToInt64(document.Values.FirstOrDefault() ?? 0);

                statistics.DocumentsCount += result.Count();
                statistics.TotalImagesCount += result.Sum(x => DocumentHasImage(x));

                statistics.TotalImagesSizeBytes += _imageFileSizeStatistics.GetTotalFileSize(productionId, workspaceId);

                return statistics;
            }
            try
            {
                QueryRequest query = new ProductionInformationQueryBuilder()
                    .AddProductionSetCondition(productionId)
                    .AddField(ProductionConsts.ImageCountFieldGuid)
                    .Build();

                DocumentsStatistics statistics = await QueryDocumentsWithExport(workspaceId, query,
                    nameof(GetImagesStatisticsForProductionAsync), token,
                    StatisticsCalculation).ConfigureAwait(false);

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

        private async Task<DocumentsStatistics> QueryDocumentsWithExport(int workspaceId, QueryRequest query,
            string callingMethodName, CancellationToken token, Func<DocumentsStatistics, IEnumerable<RelativityObjectSlim>, DocumentsStatistics> function)
        {
            IRelativityObjectManager objectManager =
                _relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceId);

            Stopwatch stopwatch = Stopwatch.StartNew();
            using (IExportQueryResult export = await objectManager.QueryWithExportAsync(query, 0).ConfigureAwait(false))
            {
                stopwatch.Restart();
                _logger.LogInformation("Exported {count} documents ({method}) in {elapsed} s",
                    export.ExportResult.RecordCount, callingMethodName, stopwatch.Elapsed.TotalSeconds);

                DocumentsStatistics statistics = new DocumentsStatistics();
                IEnumerable<RelativityObjectSlim> result;
                int startIndex = 0;
                do
                {
                    result = (await export.GetNextBlockAsync(startIndex, token).ConfigureAwait(false));
                    if (result.Any())
                    {
                        statistics = function(statistics, result);
                    }
                    startIndex += result.Count();
                }
                while (result.Any());

                stopwatch.Stop();
                _logger.LogInformation("Calculated total items size for method {callingMethodName} in {elapsed} ms",
                    callingMethodName, stopwatch.ElapsedMilliseconds);

                return statistics;
            }
        }
    }
}