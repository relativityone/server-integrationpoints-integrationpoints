using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public sealed class SourceWorkspaceTagCreator
	{
		private readonly IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private readonly IWorkspaceRepository _workspaceRepository;
		private readonly IFederatedInstanceManager _federatedInstanceManager;
		private readonly IAPILog _logger;

		public SourceWorkspaceTagCreator(IDestinationWorkspaceRepository destinationWorkspaceRepository, IWorkspaceRepository workspaceRepository,
			IFederatedInstanceManager federatedInstanceManager, IAPILog logger)
		{
			_destinationWorkspaceRepository = destinationWorkspaceRepository;
			_workspaceRepository = workspaceRepository;
			_federatedInstanceManager = federatedInstanceManager;
			_logger = logger;
		}

		public int CreateDestinationWorkspaceTag(int destinationWorkspaceId, int jobHistoryInstanceId, int? federatedInstanceId)
		{
			DestinationWorkspace destinationWorkspace = _destinationWorkspaceRepository.Query(destinationWorkspaceId, federatedInstanceId);
			string destinationWorkspaceName = _workspaceRepository.Retrieve(destinationWorkspaceId).Name;
			string destinationInstanceName = _federatedInstanceManager.RetrieveFederatedInstanceByArtifactId(federatedInstanceId).Name;

			if (destinationWorkspace == null)
			{
				LogCreatingDestinationWorkspace(destinationInstanceName, destinationWorkspaceId, federatedInstanceId);
				destinationWorkspace = _destinationWorkspaceRepository.Create(destinationWorkspaceId, destinationWorkspaceName, federatedInstanceId, destinationInstanceName);
			}
			else if (destinationWorkspaceName != destinationWorkspace.DestinationWorkspaceName || destinationInstanceName != destinationWorkspace.DestinationInstanceName)
			{
				LogDestinationWorkspaceUpdate(destinationWorkspace, destinationWorkspaceName, destinationInstanceName);
				destinationWorkspace.DestinationWorkspaceName = destinationWorkspaceName;
				destinationWorkspace.DestinationInstanceName = destinationInstanceName;
				_destinationWorkspaceRepository.Update(destinationWorkspace);
			}

			int destinationWorkspaceTagArtifactId = destinationWorkspace.ArtifactId;
			_destinationWorkspaceRepository.LinkDestinationWorkspaceToJobHistory(destinationWorkspaceTagArtifactId, jobHistoryInstanceId);
			return destinationWorkspaceTagArtifactId;
		}

		private void LogCreatingDestinationWorkspace(string destinationInstanceName, int destinationWorkspaceId, int? federatedInstanceId)
		{
			_logger.LogInformation("Creating destination workspace: {destinationWorkspaceName}, {_destinationWorkspaceId}. Destination instance: {destinationInstanceName},{_federatedInstanceId}",
				destinationInstanceName, destinationWorkspaceId, destinationInstanceName, federatedInstanceId);
		}

		private void LogDestinationWorkspaceUpdate(DestinationWorkspace destinationWorkspace, string destinationWorkspaceName, string destinationInstanceName)
		{
			_logger.LogInformation("Updating destination workspace. Old:{oldInstace},{oldWorkspace}; new: {newInstance}, {newWorkspace}",
				destinationWorkspace.DestinationInstanceName, destinationWorkspace.DestinationWorkspaceName, destinationInstanceName, destinationWorkspaceName);
		}
	}
}