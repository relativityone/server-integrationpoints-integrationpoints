using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
    internal abstract class WorkspaceTagRepositoryBase<TIdentifier> : IWorkspaceTagRepository<TIdentifier>
    {
        private const int _MAX_OBJECT_QUERY_BATCH_SIZE = 10000;

        private readonly IAPILog _logger;

        protected const string _UNIT_OF_MEASURE = "document(s)";

        protected WorkspaceTagRepositoryBase(IAPILog logger)
        {
            _logger = logger;
        }

        public async Task<IList<TagDocumentsResult<TIdentifier>>> TagDocumentsAsync(ISynchronizationConfiguration synchronizationConfiguration, IList<TIdentifier> documentIdentifiers, CancellationToken token)
        {
            var tagResults = new List<TagDocumentsResult<TIdentifier>>();
            if (documentIdentifiers.Count == 0)
            {
                const string noUpdateMessage = "A call to the Mass Update API was not made as there are no objects to update.";
                var result = new TagDocumentsResult<TIdentifier>(documentIdentifiers, noUpdateMessage, true, documentIdentifiers.Count);
                tagResults.Add(result);
                return tagResults;
            }

            IEnumerable<FieldRefValuePair> fieldValues = GetDocumentFieldTags(synchronizationConfiguration);
            var massUpdateOptions = new MassUpdateOptions
            {
                UpdateBehavior = FieldUpdateBehavior.Merge
            };
            IEnumerable<IList<TIdentifier>> documentArtifactIdBatches = documentIdentifiers.SplitList(_MAX_OBJECT_QUERY_BATCH_SIZE);
            foreach (IList<TIdentifier> documentArtifactIdBatch in documentArtifactIdBatches)
            {
                TagDocumentsResult<TIdentifier> tagResult = await TagDocumentsBatchAsync(synchronizationConfiguration, documentArtifactIdBatch, fieldValues, massUpdateOptions, token).ConfigureAwait(false);
                tagResults.Add(tagResult);
            }
            return tagResults;
        }

        protected abstract Task<TagDocumentsResult<TIdentifier>> TagDocumentsBatchAsync(
            ISynchronizationConfiguration synchronizationConfiguration, IList<TIdentifier> batch,
            IEnumerable<FieldRefValuePair> fieldValues, MassUpdateOptions massUpdateOptions, CancellationToken token);

        protected abstract FieldRefValuePair[] GetDocumentFieldTags(ISynchronizationConfiguration synchronizationConfiguration);

        protected static IEnumerable<RelativityObjectRef> ToMultiObjectValue(params int[] artifactIds)
        {
            return artifactIds.Select(x => new RelativityObjectRef { ArtifactID = x });
        }

        protected TagDocumentsResult<TIdentifier> GenerateTagDocumentsResult(MassUpdateResult updateResult, IList<TIdentifier> batch)
        {
            IEnumerable<TIdentifier> failedDocumentArtifactIds;
            if (!updateResult.Success)
            {
                int elementsToCapture = batch.Count - updateResult.TotalObjectsUpdated;
                failedDocumentArtifactIds = batch.ToList().GetRange(updateResult.TotalObjectsUpdated, elementsToCapture);

                const string massUpdateErrorTemplate = "A response to a request for mass tagging synchronized documents in workspace indicates that an error has occurred while processing the request: {MassUpdateResultMessage}. Successfully tagged {MassUpdateResultTotalObjectsUpdated} of {BatchCount} documents.";

                _logger.LogError(massUpdateErrorTemplate, updateResult.Message, updateResult.TotalObjectsUpdated, batch.Count);
            }
            else
            {
                failedDocumentArtifactIds = Array.Empty<TIdentifier>();
            }
            var result = new TagDocumentsResult<TIdentifier>(failedDocumentArtifactIds, updateResult.Message, updateResult.Success, updateResult.TotalObjectsUpdated);
            return result;
        }
    }
}
