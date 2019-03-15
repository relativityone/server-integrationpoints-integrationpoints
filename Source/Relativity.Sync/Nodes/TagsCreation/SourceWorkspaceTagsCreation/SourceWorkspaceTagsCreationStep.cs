using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes.TagsCreation.SourceWorkspaceTagsCreation
{
	internal sealed class SourceWorkspaceTagsCreationStep : IExecutor<ISourceWorkspaceTagsCreationConfiguration>, IExecutionConstrains<ISourceWorkspaceTagsCreationConfiguration>
	{
		private readonly IDestinationWorkspaceTagQuery _destinationWorkspaceTagQuery;
		private readonly IAPILog _logger;
		private readonly IWorkspaceNameQuery _workspaceNameQuery;

		public SourceWorkspaceTagsCreationStep(IDestinationWorkspaceTagQuery destinationWorkspaceTagQuery, IWorkspaceNameQuery workspaceNameQuery, IAPILog logger)
		{
			_destinationWorkspaceTagQuery = destinationWorkspaceTagQuery;
			_workspaceNameQuery = workspaceNameQuery;
			_logger = logger;
		}

		public Task<bool> CanExecuteAsync(ISourceWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(true);
		}

		public async Task ExecuteAsync(ISourceWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			int destinationWorkspaceTagArtifactId = await CreateOrUpdateDestinationWorkspaceTag(configuration).ConfigureAwait(false);
			configuration.SetDestinationWorkspaceTagArtifactId(destinationWorkspaceTagArtifactId);
		}

		private async Task<int> CreateOrUpdateDestinationWorkspaceTag(ISourceWorkspaceTagsCreationConfiguration configuration)
		{
			string destinationWorkspaceName = await _workspaceNameQuery.GetWorkspaceNameAsync(configuration.DestinationWorkspaceArtifactId).ConfigureAwait(false);
			string destinationInstanceName = "some other instance";

			DestinationWorkspaceTag tag = await _destinationWorkspaceTagQuery.QueryAsync(configuration).ConfigureAwait(false);
			if (tag == null)
			{
				tag = await CreateDestinationWorkspaceTagAsync(configuration).ConfigureAwait(false);
			}
			else if (!destinationWorkspaceName.Equals(tag.DestinationWorkspaceName, StringComparison.InvariantCulture) ||
				!destinationInstanceName.Equals(tag.DestinationInstanceName, StringComparison.InvariantCulture))
			{
				tag.DestinationWorkspaceName = destinationWorkspaceName;
				tag.DestinationInstanceName = destinationInstanceName;
				await UpdateDestinationWorkspaceTagAsync(tag).ConfigureAwait(false);
			}

			await LinkDestinationWorkspaceToJobHistoryAsync(tag.ArtifactId, configuration.JobArtifactId).ConfigureAwait(false);
			return tag.ArtifactId;
		}

		private Task LinkDestinationWorkspaceToJobHistoryAsync(int tagArtifactId, int jobArtifactId)
		{
			_logger.LogVerbose("Linking destination workspace tag {tagArtifactId} to job {jobArtifactId}", tagArtifactId, jobArtifactId);
			throw new NotImplementedException();
		}

		private Task<DestinationWorkspaceTag> CreateDestinationWorkspaceTagAsync(ISourceWorkspaceTagsCreationConfiguration configuration)
		{

			throw new NotImplementedException();
		}

		private Task UpdateDestinationWorkspaceTagAsync(DestinationWorkspaceTag tag)
		{
			throw new NotImplementedException();
		}
	}
}