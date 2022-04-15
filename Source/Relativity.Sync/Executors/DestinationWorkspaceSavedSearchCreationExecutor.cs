using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceSavedSearchCreationExecutor : IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration>
	{
		private readonly ITagSavedSearch _tagSavedSearch;
		private readonly ITagSavedSearchFolder _tagSavedSearchFolder;
		private readonly IAPILog _logger;

		public DestinationWorkspaceSavedSearchCreationExecutor(ITagSavedSearch tagSavedSearch, ITagSavedSearchFolder tagSavedSearchFolder, IAPILog logger)
		{
			_tagSavedSearch = tagSavedSearch;
			_tagSavedSearchFolder = tagSavedSearchFolder;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IDestinationWorkspaceSavedSearchCreationConfiguration configuration, CompositeCancellationToken token)
		{
			_logger.LogInformation("Creating saved search in destination workspace artifact ID: {destinationWorkspaceArtifactId}", configuration.DestinationWorkspaceArtifactId);

			ExecutionResult result = ExecutionResult.Success();

			try
			{
				int savedSearchFolderArtifactId = await _tagSavedSearchFolder.GetFolderIdAsync(configuration.DestinationWorkspaceArtifactId).ConfigureAwait(false);
				int savedSearchId = await _tagSavedSearch.CreateTagSavedSearchAsync(configuration, savedSearchFolderArtifactId, token.StopCancellationToken).ConfigureAwait(false);

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
