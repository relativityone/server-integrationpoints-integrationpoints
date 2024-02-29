using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
    internal sealed class SourceWorkspaceTagsCreationExecutor : IExecutor<ISourceWorkspaceTagsCreationConfiguration>
    {
        private readonly IDestinationWorkspaceTagRepository _destinationWorkspaceTagRepository;
        private readonly IDestinationWorkspaceTagsLinker _destinationWorkspaceTagsLinker;
        private readonly IWorkspaceNameQuery _workspaceNameQuery;
        private readonly IFederatedInstance _federatedInstance;
        private readonly IDestinationServiceFactoryForUser _serviceFactory;

        public SourceWorkspaceTagsCreationExecutor(
            IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository,
            IDestinationWorkspaceTagsLinker destinationWorkspaceTagsLinker,
            IWorkspaceNameQuery workspaceNameQuery,
            IFederatedInstance federatedInstance,
            IDestinationServiceFactoryForUser serviceFactory)
        {
            _destinationWorkspaceTagRepository = destinationWorkspaceTagRepository;
            _destinationWorkspaceTagsLinker = destinationWorkspaceTagsLinker;
            _workspaceNameQuery = workspaceNameQuery;
            _federatedInstance = federatedInstance;
            _serviceFactory = serviceFactory;
        }

        public async Task<ExecutionResult> ExecuteAsync(ISourceWorkspaceTagsCreationConfiguration configuration, CompositeCancellationToken token)
        {
            ExecutionResult result = ExecutionResult.Success();
            try
            {
                int destinationWorkspaceTagArtifactId = await CreateOrUpdateDestinationWorkspaceTagAsync(configuration, token.StopCancellationToken).ConfigureAwait(false);
                await configuration.SetDestinationWorkspaceTagArtifactIdAsync(destinationWorkspaceTagArtifactId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result = ExecutionResult.Failure("Failed to create tags in source workspace", ex);
            }

            return result;
        }

        private async Task<int> CreateOrUpdateDestinationWorkspaceTagAsync(ISourceWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
        {
            string destinationWorkspaceName = await _workspaceNameQuery.GetWorkspaceNameAsync(_serviceFactory, configuration.DestinationWorkspaceArtifactId, token).ConfigureAwait(false);
            string destinationInstanceName = await _federatedInstance.GetInstanceNameAsync().ConfigureAwait(false);

            DestinationWorkspaceTag tag = await _destinationWorkspaceTagRepository.ReadAsync(configuration.SourceWorkspaceArtifactId, configuration.DestinationWorkspaceArtifactId, token).ConfigureAwait(false);
            if (tag == null)
            {
                tag = await _destinationWorkspaceTagRepository.CreateAsync(configuration.SourceWorkspaceArtifactId, configuration.DestinationWorkspaceArtifactId, destinationWorkspaceName).ConfigureAwait(false);
            }
            else if (tag.RequiresUpdate(destinationWorkspaceName, destinationInstanceName))
            {
                tag.DestinationWorkspaceName = destinationWorkspaceName;
                tag.DestinationInstanceName = destinationInstanceName;
                await _destinationWorkspaceTagRepository.UpdateAsync(configuration.SourceWorkspaceArtifactId, tag).ConfigureAwait(false);
            }

            await _destinationWorkspaceTagsLinker.LinkDestinationWorkspaceTagToJobHistoryAsync(
                configuration.SourceWorkspaceArtifactId, tag.ArtifactId, configuration.JobHistoryArtifactId).ConfigureAwait(false);

            return tag.ArtifactId;
        }
    }
}
