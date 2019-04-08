using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceTagsCreationExecutor : IExecutor<IDestinationWorkspaceTagsCreationConfiguration>
	{
		private readonly ISourceCaseTagService _sourceCaseTagService;
		private readonly ISourceJobTagService _sourceJobTagService;
		private readonly ISyncLog _logger;

		public DestinationWorkspaceTagsCreationExecutor(ISourceCaseTagService sourceCaseTagService, ISourceJobTagService sourceJobTagService, ISyncLog logger)
		{
			_sourceCaseTagService = sourceCaseTagService;
			_sourceJobTagService = sourceJobTagService;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			ExecutionResult result = ExecutionResult.Success();

			try
			{
				RelativitySourceCaseTag sourceCaseTag = await _sourceCaseTagService.CreateOrUpdateSourceCaseTagAsync(configuration, token).ConfigureAwait(false);
				configuration.SetSourceWorkspaceTag(sourceCaseTag.ArtifactId, sourceCaseTag.Name);

				RelativitySourceJobTag sourceJobTag = await _sourceJobTagService.CreateSourceJobTagAsync(configuration, sourceCaseTag.ArtifactId, token).ConfigureAwait(false);
				configuration.SetSourceJobTag(sourceJobTag.ArtifactId, sourceJobTag.Name);
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
