using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.FederatedInstance;
using Relativity.Sync.Executors.Repository;
using Relativity.Sync.Executors.TagsCreation;

namespace Relativity.Sync.Executors.SourceWorkspaceTagsCreation
{
	internal sealed class SourceWorkspaceTagsCreationStep : IExecutor<ISourceWorkspaceTagsCreationConfiguration>
	{
		private readonly IDestinationWorkspaceTagRepository _destinationWorkspaceTagRepository;
		private readonly IDestinationWorkspaceTagsLinker _destinationWorkspaceTagsLinker;
		private readonly IWorkspaceNameQuery _workspaceNameQuery;
		private readonly IFederatedInstance _federatedInstance;

		public SourceWorkspaceTagsCreationStep(IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository,
			IDestinationWorkspaceTagsLinker destinationWorkspaceTagsLinker, IWorkspaceNameQuery workspaceNameQuery, IFederatedInstance federatedInstance)
		{
			_destinationWorkspaceTagRepository = destinationWorkspaceTagRepository;
			_destinationWorkspaceTagsLinker = destinationWorkspaceTagsLinker;
			_workspaceNameQuery = workspaceNameQuery;
			_federatedInstance = federatedInstance;
		}

		public async Task ExecuteAsync(ISourceWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			int destinationWorkspaceTagArtifactId = await CreateOrUpdateDestinationWorkspaceTag(configuration).ConfigureAwait(false);
			configuration.SetDestinationWorkspaceTagArtifactId(destinationWorkspaceTagArtifactId);
		}

		private async Task<int> CreateOrUpdateDestinationWorkspaceTag(ISourceWorkspaceTagsCreationConfiguration configuration)
		{
			string destinationWorkspaceName = await _workspaceNameQuery.GetWorkspaceNameAsync(configuration.DestinationWorkspaceArtifactId).ConfigureAwait(false);
			string destinationInstanceName = await _federatedInstance.GetInstanceNameAsync().ConfigureAwait(false);

			DestinationWorkspaceTag tag = await _destinationWorkspaceTagRepository.ReadAsync(configuration.SourceWorkspaceArtifactId, configuration.DestinationWorkspaceArtifactId).ConfigureAwait(false);
			if (tag == null)
			{
				tag = await _destinationWorkspaceTagRepository.CreateAsync(configuration.SourceWorkspaceArtifactId, configuration.DestinationWorkspaceArtifactId, destinationWorkspaceName).ConfigureAwait(false);
			}
			else if (ShouldUpdateDestinationWorkspaceTag(tag, destinationWorkspaceName, destinationInstanceName))
			{
				tag.DestinationWorkspaceName = destinationWorkspaceName;
				tag.DestinationInstanceName = destinationInstanceName;
				await _destinationWorkspaceTagRepository.UpdateAsync(configuration.SourceWorkspaceArtifactId, tag).ConfigureAwait(false);
			}

			await _destinationWorkspaceTagsLinker.LinkDestinationWorkspaceTagToJobHistoryAsync(
				configuration.SourceWorkspaceArtifactId, configuration.DestinationWorkspaceArtifactId, configuration.JobArtifactId).ConfigureAwait(false);

			configuration.SetDestinationWorkspaceTagArtifactId(tag.ArtifactId);

			return tag.ArtifactId;
		}

		private static bool ShouldUpdateDestinationWorkspaceTag(DestinationWorkspaceTag tag, string destinationWorkspaceName, string destinationInstanceName)
		{
			return !destinationWorkspaceName.Equals(tag.DestinationWorkspaceName, StringComparison.InvariantCulture) ||
				!destinationInstanceName.Equals(tag.DestinationInstanceName, StringComparison.InvariantCulture);
		}
	}
}