using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Kepler.Document;
using Relativity.Sync.Kepler.SyncBatch;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors.Tagging
{
    internal class SourceWorkspaceTaggingService : ISourceWorkspaceTaggingService
    {
        private const string _TAGGING_FAILED_ERROR_MESSAGE = "Document transferred successfully, but tagging the source workspace failed. Please retry the job in order to tag the document in the source workspace.";
        private const string _NO_UPDATE_MESSAGE = "A call to the Mass Update API was not made as there are no objects to update.";
        private const string _UNIT_OF_MEASURE = "document(s)";

        private readonly IRelativityExportBatcherFactory _exportBatcherFactory;
        private readonly IAPILog _logger;
        private readonly IStopwatch _stopwatch;
        private readonly IIdentifierFieldMapService _identifierFieldMapService;
        private readonly IDocumentRepository _documentRepository;
        private readonly IInstanceSettingsDocument _instanceSettingsDoc;
        private readonly ISyncMetrics _syncMetrics;
        private readonly ITaggingRepository _tagRepository;

        public SourceWorkspaceTaggingService(
            IRelativityExportBatcherFactory exportBatcherFactory,
            IStopwatch stopwatch,
            IAPILog logger,
            IDocumentRepository documentRepository,
            ISyncMetrics syncMetrics,
            IInstanceSettingsDocument instanceSettingsDoc,
            ITaggingRepository tagRepository,
            IIdentifierFieldMapService identifierFieldMapService)
        {
            _exportBatcherFactory = exportBatcherFactory;
            _stopwatch = stopwatch;
            _logger = logger;
            _documentRepository = documentRepository;
            _syncMetrics = syncMetrics;
            _instanceSettingsDoc = instanceSettingsDoc;
            _identifierFieldMapService = identifierFieldMapService;
            _tagRepository = tagRepository;
        }

        public async Task<TaggingExecutionResult> TagDocumentsInSourceWorkspaceAsync(ISynchronizationConfiguration configuration, SyncBatchDto syncBatch)
        {
            _logger.LogInformation(
                "Start Tagging in SourceWorkspace {workspaceId} for Batch {batchId}...", configuration.SourceWorkspaceArtifactId, syncBatch.ArtifactId);

            try
            {
                List<DocumentDto> documentIds = await GetDocumentsForTaggingAsync(syncBatch, Get_exportBatcherFactory()).ConfigureAwait(false);

                return await TagDocumentsInSourceWorkspaceInternalAsync(documentIds, configuration).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Tagging in Source Workspace has failed for syncBatch {batchId}", syncBatch.ArtifactId);
                return TaggingExecutionResult.Failure(e.Message, e);
            }
        }

        private IRelativityExportBatcherFactory Get_exportBatcherFactory()
        {
            return _exportBatcherFactory;
        }

        private async Task<List<DocumentDto>> GetDocumentsForTaggingAsync(SyncBatchDto syncBatch, IRelativityExportBatcherFactory _exportBatcherFactory)
        {
            IStopwatch sw = _stopwatch.StartNew();

            FieldMap identifierField = _identifierFieldMapService.GetObjectIdentifierField();

            _logger.LogInformation("Retrieving Document IDs for tagging for syncBatch {batchId} based on identifier {@identifier}", syncBatch.ArtifactId, identifierField.FieldIndex);

            List<DocumentDto> documents = new List<DocumentDto>();
            IRelativityExportBatcher export = _exportBatcherFactory.CreateRelativityExportBatchForTagging(syncBatch);

            RelativityObjectSlim[] objects;
            do
            {
                objects = await export.GetNextItemsFromBatchAsync(CancellationToken.None).ConfigureAwait(false);
                if (objects is null)
                {
                    break;
                }

                documents.AddRange(objects.Select(x => new DocumentDto
                {
                    ArtifactId = x.ArtifactID,
                    Identifier = x.Values[identifierField.FieldIndex].ToString()
                }));
            }
            while (objects.Length > 0);

            List<DocumentDto> erroredDocuments = (await _documentRepository
                    .GetErroredDocumentsByBatchAsync(syncBatch, Identity.CurrentUser)
                    .ConfigureAwait(false))
                .Select(x => new DocumentDto { ArtifactId = x })
                .ToList();

            documents = documents.Except(erroredDocuments).ToList();

            sw.Stop();

            _logger.LogInformation(
                "Retrieved {documentsCount} documents from Batch {batchId} for tagging. Errored Documents Count: {erroredCount}, Duration: {duration}",
                documents.Count,
                syncBatch.ArtifactId,
                erroredDocuments.Count,
                sw.Elapsed);

            return documents;
        }

        private async Task<TaggingExecutionResult> TagDocumentsInSourceWorkspaceInternalAsync(
            List<DocumentDto> documents,
            ISynchronizationConfiguration configuration)
        {
            List<int> documentIdentifiers = documents.Select(x => x.ArtifactId).ToList();

            var taggingResults = new List<TagDocumentsResult<int>>();
            if (documentIdentifiers.Count == 0)
            {
                var tagDocumentsResult = new TagDocumentsResult<int>(documentIdentifiers, _NO_UPDATE_MESSAGE, true, documentIdentifiers.Count);
                taggingResults.Add(tagDocumentsResult);
            }

            int documentTaggingBatchSize = await _instanceSettingsDoc.GetSyncDocumentTaggingBatchSizeAsync().ConfigureAwait(false);
            IEnumerable<List<int>> documentArtifactIdBatches = documentIdentifiers.Chunk(documentTaggingBatchSize);
            foreach (List<int> documentArtifactIdBatch in documentArtifactIdBatches)
            {
                TagDocumentsResult<int> tagResult = await TagDocumentsBatchAsync(configuration, documentArtifactIdBatch).ConfigureAwait(false);
                taggingResults.Add(tagResult);
            }

            TagDocumentsResult<int> taggingResult = TagDocumentsResult<int>.Merge(taggingResults);
            _logger.LogInformation("Documents were tagged with following result - {@taggingResult}", taggingResult);

            TaggingExecutionResult result = GetTaggingExecutionResult(documents, taggingResult);

            return result;
        }

        private async Task<TagDocumentsResult<int>> TagDocumentsBatchAsync(
            ISynchronizationConfiguration synchronizationConfiguration, List<int> documentIds)
        {
            IStopwatch stopwatch = _stopwatch.StartNew();
            stopwatch.Start();

            TagDocumentsResult<int> result = await TagDocumentsAsync(
                synchronizationConfiguration.SourceWorkspaceArtifactId,
                documentIds,
                synchronizationConfiguration.DestinationWorkspaceTagArtifactId,
                synchronizationConfiguration.JobHistoryArtifactId)
                .ConfigureAwait(false);
            stopwatch.Stop();

            _syncMetrics.Send(new DestinationWorkspaceTagMetric
            {
                BatchSize = documentIds.Count,
                SourceUpdateTime = stopwatch.Elapsed.TotalMilliseconds,
                SourceUpdateCount = result.TotalObjectsUpdated,
                UnitOfMeasure = _UNIT_OF_MEASURE
            });

            return result;
        }

        private async Task<TagDocumentsResult<int>> TagDocumentsAsync(int workspaceId, List<int> documentsIds, int destinationWorkspaceTagId, int jobHistoryId)
        {
            TagDocumentsResult<int> result;
            try
            {
                MassUpdateResult updateResult = await _tagRepository.TagDocumentsAsync(workspaceId, documentsIds, destinationWorkspaceTagId, jobHistoryId).ConfigureAwait(false);

                result = GenerateTagDocumentsResult(updateResult, documentsIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Documents Tagging in Workspace {workspaceId} failed.", workspaceId);
                result = new TagDocumentsResult<int>(documentsIds, $"Documents Tagging in Workspace {workspaceId} failed.", false, 0);
            }

            return result;
        }

        private TagDocumentsResult<int> GenerateTagDocumentsResult(MassUpdateResult updateResult, List<int> documentsIds)
        {
            IEnumerable<int> failedDocumentArtifactIds;
            if (!updateResult.Success)
            {
                int elementsToCapture = documentsIds.Count - updateResult.TotalObjectsUpdated;
                failedDocumentArtifactIds = documentsIds.ToList().GetRange(updateResult.TotalObjectsUpdated, elementsToCapture);

                const string massUpdateErrorTemplate = "A response to a request for mass tagging synchronized documents in workspace indicates that an error has occurred while processing the request: {MassUpdateResultMessage}. Successfully tagged {MassUpdateResultTotalObjectsUpdated} of {BatchCount} documents.";

                _logger.LogError(massUpdateErrorTemplate, updateResult.Message, updateResult.TotalObjectsUpdated, documentsIds.Count);
            }
            else
            {
                failedDocumentArtifactIds = Array.Empty<int>();
            }

            var result = new TagDocumentsResult<int>(failedDocumentArtifactIds, updateResult.Message, updateResult.Success, updateResult.TotalObjectsUpdated);
            return result;
        }

        private IEnumerable<JobHistoryCreateItemError> GetItemLevelErrors(List<DocumentDto> failedTaggedDocuments)
        {
            return failedTaggedDocuments.Select(doc => new JobHistoryCreateItemError
            {
                ErrorMessage = _TAGGING_FAILED_ERROR_MESSAGE,
                SourceUniqueId = doc.Identifier,
            });
        }

        private TaggingExecutionResult GetTaggingExecutionResult(List<DocumentDto> documentIds, TagDocumentsResult<int> taggingResult)
        {
            TaggingExecutionResult result = TaggingExecutionResult.Success();
            if (!taggingResult.Success)
            {
                IEnumerable<DocumentDto> erroredDocuments = taggingResult.FailedDocuments.Select(x => new DocumentDto { ArtifactId = x });
                List<DocumentDto> failedTaggedDocuments = documentIds.Intersect(erroredDocuments).ToList();

                _logger.LogInformation("Failed to tag {documentsCount} documents.", failedTaggedDocuments.Count);

                result = TaggingExecutionResult.SuccessWithErrors();
                result.FailedDocuments = GetItemLevelErrors(failedTaggedDocuments).ToList();
            }

            result.TaggedDocumentsCount = taggingResult.TotalObjectsUpdated;
            return result;
        }

    }
}