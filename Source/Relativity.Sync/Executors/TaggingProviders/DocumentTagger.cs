using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors.TaggingProviders
{
    internal class DocumentTagger : ITaggingProvider
    {
	    private readonly ISyncLog _logger;
	    private readonly IDocumentTagRepository _documentsTagRepository;

	    public DocumentTagger(IDocumentTagRepository documentTagRepository, ISyncLog logger)
	    {
		    _documentsTagRepository = documentTagRepository;
		    _logger = logger;
	    }

	    public async Task<TaggingExecutionResult> TagObjectsAsync(IImportJob importJob,
		    ISynchronizationConfiguration configuration, CompositeCancellationToken token)
	    {
		    Task<TaggingExecutionResult> destinationDocumentsTaggingTask = TagDestinationDocumentsAsync(importJob, configuration, token.StopCancellationToken);
			Task<TaggingExecutionResult> sourceDocumentsTaggingTask = TagSourceDocumentsAsync(importJob, configuration, token.StopCancellationToken);

			TaggingExecutionResult sourceTaggingResult = await sourceDocumentsTaggingTask.ConfigureAwait(false);
			TaggingExecutionResult destinationTaggingResult = await destinationDocumentsTaggingTask.ConfigureAwait(false);

			TaggingExecutionResult taggingExecutionResult = TaggingExecutionResult.Compose(sourceTaggingResult, destinationTaggingResult);

			return taggingExecutionResult;
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
			_logger.LogInformation("Start tagging documents in source workspace ArtifactID: {workspaceID}", configuration.DestinationWorkspaceArtifactId);
			List<int> pushedDocumentArtifactIds = (await importJob.GetPushedDocumentArtifactIdsAsync().ConfigureAwait(false)).ToList();
			_logger.LogInformation("Number of pushed documents to tag: {numberOfDocuments}", pushedDocumentArtifactIds.Count);

			TaggingExecutionResult taggingResult =
				await _documentsTagRepository.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(configuration, pushedDocumentArtifactIds, token).ConfigureAwait(false);

			_logger.LogInformation("Documents tagging in source workspace ArtifactID: {workspaceID} Result: {result}", configuration.DestinationWorkspaceArtifactId,
				taggingResult.Status);

			return taggingResult;
		}
	}
}
