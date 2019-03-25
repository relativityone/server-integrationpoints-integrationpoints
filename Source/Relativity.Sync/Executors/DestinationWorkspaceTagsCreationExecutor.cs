using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceTagsCreationExecutor : IExecutor<IDestinationWorkspaceTagsCreationConfiguration>
	{
		private readonly ISourceCaseTagService _sourceCaseTagService;
		private readonly ISourceJobTagService _sourceJobTagService;

		public DestinationWorkspaceTagsCreationExecutor(ISourceCaseTagService sourceCaseTagService, ISourceJobTagService sourceJobTagService)
		{
			_sourceCaseTagService = sourceCaseTagService;
			_sourceJobTagService = sourceJobTagService;
		}

		public async Task ExecuteAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			RelativitySourceCaseTag sourceCaseTag = await _sourceCaseTagService.CreateOrUpdateSourceCaseTagAsync(configuration, token).ConfigureAwait(false);
			configuration.SetSourceWorkspaceTag(sourceCaseTag.ArtifactId, sourceCaseTag.Name);

			RelativitySourceJobTag sourceJobTag = await _sourceJobTagService.CreateSourceJobTagAsync(configuration, sourceCaseTag.ArtifactId, token).ConfigureAwait(false);
			configuration.SetSourceJobTag(sourceJobTag.ArtifactId, sourceJobTag.Name);
		}
	}
}
