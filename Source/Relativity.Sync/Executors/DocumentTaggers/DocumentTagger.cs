using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors.DocumentTaggers
{
    internal class DocumentTagger : IDocumentTagger
    {
	    private readonly IAPILog _logger;
	    private readonly IDocumentTagRepository _documentsTagRepository;

	    public DocumentTagger(IDocumentTagRepository documentTagRepository, IAPILog logger)
	    {
		    _documentsTagRepository = documentTagRepository;
		    _logger = logger;
	    }

        public async Task<TaggingExecutionResult> TagObjectsAsync(IImportJob importJob,
            ISynchronizationConfiguration configuration, CompositeCancellationToken token)
        {
            Task<TaggingExecutionResult> destinationDocumentsTaggingTask =
                TagDestinationDocumentsAsync(importJob, configuration, CancellationToken.None);
            Task<TaggingExecutionResult> sourceDocumentsTaggingTask =
                TagSourceDocumentsAsync(importJob, configuration, CancellationToken.None);

            if (token.IsStopRequested)
            {
                string message =
                    $"Sync job was cancelled. SyncConfigurationArtifactId - {configuration.SyncConfigurationArtifactId}, ExportRunId - {configuration.ExportRunId}";
                return new TaggingExecutionResult(ExecutionStatus.Canceled, message, new Exception());
            }

            TaggingExecutionResult sourceTaggingResult = await sourceDocumentsTaggingTask.ConfigureAwait(false);
            TaggingExecutionResult destinationTaggingResult = await destinationDocumentsTaggingTask.ConfigureAwait(false);

            return TaggingExecutionResult.Compose(sourceTaggingResult, destinationTaggingResult);
        }

        private async Task<TaggingExecutionResult> TagDestinationDocumentsAsync(IImportJob importJob, ISynchronizationConfiguration configuration,
			CancellationToken token)
		{
			_logger.LogInformation("Start tagging documents in destination workspace ArtifactID: {workspaceID}", configuration.DestinationWorkspaceArtifactId);
			List<string> pushedDocumentIdentifiers = (await importJob.GetPushedDocumentIdentifiersAsync().ConfigureAwait(false)).ToList();
			_logger.LogInformation("Number of pushed documents to tag: {numberOfDocuments}", pushedDocumentIdentifiers.Count);
			TaggingExecutionResult taggingResult =
				await _documentsTagRepository.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(configuration, pushedDocumentIdentifiers, token).ConfigureAwait(false);

			_logger.LogInformation("Documents tagging in destination workspace ArtifactID: {workspaceID} Result: {result}", configuration.DestinationWorkspaceArtifactId,
				taggingResult.Status);

			return taggingResult;
		}

		private async Task<TaggingExecutionResult> TagSourceDocumentsAsync(IImportJob importJob, ISynchronizationConfiguration configuration,
			CancellationToken token)
		{
			_logger.LogInformation("Start tagging documents in source workspace ArtifactID: {workspaceID}", configuration.SourceWorkspaceArtifactId);
			List<int> pushedDocumentArtifactIds = (await importJob.GetPushedDocumentArtifactIdsAsync().ConfigureAwait(false)).ToList();
			_logger.LogInformation("Number of pushed documents to tag: {numberOfDocuments}", pushedDocumentArtifactIds.Count);

			TaggingExecutionResult taggingResult =
				await _documentsTagRepository.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(configuration, pushedDocumentArtifactIds, token).ConfigureAwait(false);

			_logger.LogInformation("Documents tagging in source workspace ArtifactID: {workspaceID} Result: {result}", configuration.SourceWorkspaceArtifactId,
				taggingResult.Status);

			return taggingResult;
		}
	}
}
