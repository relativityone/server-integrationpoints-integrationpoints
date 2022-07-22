using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal sealed class DocumentTagRepository : IDocumentTagRepository
    {
        private readonly IDestinationWorkspaceTagRepository _destinationWorkspaceTagRepository;
        private readonly ISourceWorkspaceTagRepository _sourceWorkspaceTagRepository;
        private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;

        public DocumentTagRepository(IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository,
            ISourceWorkspaceTagRepository sourceWorkspaceTagRepository, IJobHistoryErrorRepository jobHistoryErrorRepository)
        {
            _destinationWorkspaceTagRepository = destinationWorkspaceTagRepository;
            _sourceWorkspaceTagRepository = sourceWorkspaceTagRepository;
            _jobHistoryErrorRepository = jobHistoryErrorRepository;
        }

        public Task<TaggingExecutionResult> TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<string> documentIdentifiers, CancellationToken token)
        {
            return TagDocumentsInWorkspaceWithInfoAsync(_sourceWorkspaceTagRepository.TagDocumentsAsync, configuration, documentIdentifiers, token);
        }

        public Task<TaggingExecutionResult> TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<int> artifactIds, CancellationToken token)
        {
            return TagDocumentsInWorkspaceWithInfoAsync(_destinationWorkspaceTagRepository.TagDocumentsAsync, configuration, artifactIds, token);
        }

        private async Task<TaggingExecutionResult> TagDocumentsInWorkspaceWithInfoAsync<TIdentifier>(
            Func<ISynchronizationConfiguration, IList<TIdentifier>, CancellationToken, Task<IList<TagDocumentsResult<TIdentifier>>>> taggingFunctionAsync,
            ISynchronizationConfiguration configuration, IEnumerable<TIdentifier> documentIdentifiers, CancellationToken token)
        {
            var taggingDocumentResult = TagDocumentsResult<TIdentifier>.Empty();

            IList<TIdentifier> documentIdentifiersList = documentIdentifiers.ToList();
            if (documentIdentifiersList.Any())
            {
                IList<TagDocumentsResult<TIdentifier>> taggingResults = await taggingFunctionAsync.Invoke(configuration, documentIdentifiersList, token).ConfigureAwait(false);
                taggingDocumentResult = TagDocumentsResult<TIdentifier>.Merge(taggingResults);
            }

            TaggingExecutionResult taggingExecutionResult = GetTaggingExecutionResult(taggingDocumentResult, configuration.JobHistoryArtifactId);
            if (taggingExecutionResult.Status == ExecutionStatus.Failed)
            {
                await GenerateDocumentTaggingJobHistoryErrorAsync(taggingExecutionResult, configuration).ConfigureAwait(false);
            }

            return taggingExecutionResult;
        }

        private TaggingExecutionResult GetTaggingExecutionResult<TIdentifier>(TagDocumentsResult<TIdentifier> taggingDocumentResult, int jobHistoryArtifactId)
        {
            TaggingExecutionResult taggingResult = TaggingExecutionResult.Success();
            var failedIdentifiers = taggingDocumentResult.FailedDocuments.ToList();
            if (failedIdentifiers.Any())
            {
                const int maxSubset = 50;
                int subsetCount = failedIdentifiers.Count < maxSubset ? failedIdentifiers.Count : maxSubset;
                string subsetArtifactIds = string.Join(",", failedIdentifiers.Take(subsetCount));

                string errorMessage = $"Failed to tag synchronized documents in workspace. The first {subsetCount} out of {failedIdentifiers.Count} are: {subsetArtifactIds}.";
                var failedTaggingException = new SyncException(errorMessage, jobHistoryArtifactId.ToString(CultureInfo.InvariantCulture));
                taggingResult = TaggingExecutionResult.Failure(errorMessage, failedTaggingException);
            }

            taggingResult.TaggedDocumentsCount = taggingDocumentResult.TotalObjectsUpdated;

            return taggingResult;
        }

        private Task GenerateDocumentTaggingJobHistoryErrorAsync(ExecutionResult taggingResult, ISynchronizationConfiguration configuration)
        {
            var jobHistoryError = new CreateJobHistoryErrorDto(ErrorType.Job)
            {
                ErrorMessage = taggingResult.Message,
                StackTrace = taggingResult.Exception?.StackTrace
            };
            return _jobHistoryErrorRepository.CreateAsync(configuration.SourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, jobHistoryError);
        }
    }
}