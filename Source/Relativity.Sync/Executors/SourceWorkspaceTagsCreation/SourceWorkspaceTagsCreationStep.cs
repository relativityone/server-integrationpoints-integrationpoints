using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Repository;
using Relativity.Sync.Executors.TagsCreation;

namespace Relativity.Sync.Executors.SourceWorkspaceTagsCreation
{
	internal sealed class SourceWorkspaceTagsCreationStep : IExecutor<ISourceWorkspaceTagsCreationConfiguration>
	{
		private readonly IDestinationWorkspaceTagRepository _destinationWorkspaceTagRepository;
		private readonly IAPILog _logger;
		private readonly IWorkspaceNameQuery _workspaceNameQuery;

		public SourceWorkspaceTagsCreationStep(IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository, IWorkspaceNameQuery workspaceNameQuery, IAPILog logger)
		{
			_destinationWorkspaceTagRepository = destinationWorkspaceTagRepository;
			_workspaceNameQuery = workspaceNameQuery;
			_logger = logger;
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

			DestinationWorkspaceTag tag = await _destinationWorkspaceTagRepository.QueryAsync(configuration.SourceWorkspaceArtifactId, configuration.DestinationWorkspaceArtifactId).ConfigureAwait(false);
			if (tag == null)
			{
				tag = await _destinationWorkspaceTagRepository.CreateAsync(configuration.SourceWorkspaceArtifactId, configuration.DestinationWorkspaceArtifactId, destinationWorkspaceName).ConfigureAwait(false);
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

		private Task UpdateDestinationWorkspaceTagAsync(DestinationWorkspaceTag tag)
		{
			throw new NotImplementedException();
		}

		private Task LinkDestinationWorkspaceToJobHistoryAsync(int tagArtifactId, int jobArtifactId)
		{
			_logger.LogVerbose("Linking destination workspace tag {tagArtifactId} to job {jobArtifactId}", tagArtifactId, jobArtifactId);
			throw new NotImplementedException();
		}

	}
}