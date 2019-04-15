﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceSavedSearchCreationExecutor : IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration>
	{
		private readonly ITagSavedSearch _tagSavedSearch;
		private readonly ITagSavedSearchFolder _tagSavedSearchFolder;
		private readonly ISyncLog _logger;

		public DestinationWorkspaceSavedSearchCreationExecutor(ITagSavedSearch tagSavedSearch, ITagSavedSearchFolder tagSavedSearchFolder, ISyncLog logger)
		{
			_tagSavedSearch = tagSavedSearch;
			_tagSavedSearchFolder = tagSavedSearchFolder;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IDestinationWorkspaceSavedSearchCreationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Creating saved search in destination workspace artifact ID: {destinationWorkspaceArtifactId}", configuration.DestinationWorkspaceArtifactId);

			ExecutionResult result = ExecutionResult.Success();

			try
			{
				int savedSearchFolderArtifactId = await _tagSavedSearchFolder.GetFolderId(configuration.DestinationWorkspaceArtifactId).ConfigureAwait(false);
				int savedSearchId = await _tagSavedSearch.CreateTagSavedSearchAsync(configuration, savedSearchFolderArtifactId, token).ConfigureAwait(false);

				await configuration.SetSavedSearchInDestinationArtifactIdAsync(savedSearchId).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				result = ExecutionResult.Failure("Failed to create saved search in destination workspace.", ex);
			}

			return result;
		}
	}
}