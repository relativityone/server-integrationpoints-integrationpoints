﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors.TaggingProviders
{
    internal class DocumentObjectBasedTaggingProvider : ITaggingProvider
    {
	    private ISyncLog _logger;
	    private IDocumentTagRepository _documentsTagRepository;

	    public async Task<TaggingExecutionResult> TagDocumentsAsync(IImportJob importJob, ISynchronizationConfiguration configuration, CompositeCancellationToken token, IDocumentTagRepository documentTagRepository, ISyncLog logger)
	    {
		    _logger = logger;
		    _documentsTagRepository = documentTagRepository;
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
