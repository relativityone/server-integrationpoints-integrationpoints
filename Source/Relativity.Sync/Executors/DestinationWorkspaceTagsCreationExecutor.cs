using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceTagsCreationExecutor : IExecutor<IDestinationWorkspaceTagsCreationConfiguration>
	{
		private readonly ISourceCaseTagService _sourceCaseTagService;
		private readonly ISourceJobTagService _sourceJobTagService;
		private readonly IAPILog _logger;

		public DestinationWorkspaceTagsCreationExecutor(ISourceCaseTagService sourceCaseTagService, ISourceJobTagService sourceJobTagService, IAPILog logger)
		{
			_sourceCaseTagService = sourceCaseTagService;
			_sourceJobTagService = sourceJobTagService;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, CompositeCancellationToken token)
		{
			_logger.LogInformation("Creating tags in destination workspace (workspace artifact id: {destinationWorkspaceArtifactId})", configuration.DestinationWorkspaceArtifactId);

			ExecutionResult result = ExecutionResult.Success();

			try
			{
				RelativitySourceCaseTag sourceCaseTag = await _sourceCaseTagService.CreateOrUpdateSourceCaseTagAsync(configuration, token.StopCancellationToken).ConfigureAwait(false);
				await configuration.SetSourceWorkspaceTagAsync(sourceCaseTag.ArtifactId, sourceCaseTag.Name).ConfigureAwait(false);

				RelativitySourceJobTag sourceJobTag = await _sourceJobTagService.CreateOrReadSourceJobTagAsync(configuration, sourceCaseTag.ArtifactId, token.StopCancellationToken).ConfigureAwait(false);
				await configuration.SetSourceJobTagAsync(sourceJobTag.ArtifactId, sourceJobTag.Name).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				const string errorMessage = "Failed to create tags in destination workspace";
				_logger.LogError(ex, errorMessage);
				result = ExecutionResult.Failure(errorMessage, ex);
			}

			return result;
		}
	}
}
