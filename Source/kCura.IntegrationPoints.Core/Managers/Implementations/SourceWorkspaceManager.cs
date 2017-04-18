using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceWorkspaceManager : ISourceWorkspaceManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public SourceWorkspaceManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public SourceWorkspaceDTO CreateSourceWorkspaceDto(int destinationWorkspaceArtifactId, int sourceWorkspaceArtifactId, int? federatedInstanceArtifactId,
			int sourceWorkspaceDescriptorArtifactTypeId)
		{
			ISourceWorkspaceRepository sourceWorkspaceRepository = _repositoryFactory.GetSourceWorkspaceRepository(destinationWorkspaceArtifactId);
			IWorkspaceRepository workspaceRepository = _repositoryFactory.GetSourceWorkspaceRepository();
			WorkspaceDTO workspaceDto = workspaceRepository.Retrieve(sourceWorkspaceArtifactId);

			string currentInstanceName = FederatedInstanceManager.LocalInstance.Name;
			if (federatedInstanceArtifactId.HasValue)
			{
				IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();
				currentInstanceName = instanceSettingRepository.GetConfigurationValue("Relativity.Authentication", "FriendlyInstanceName");
			}

			SourceWorkspaceDTO sourceWorkspaceDto = sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(sourceWorkspaceArtifactId, currentInstanceName, federatedInstanceArtifactId);

			if (sourceWorkspaceDto == null)
			{
				sourceWorkspaceDto = new SourceWorkspaceDTO
				{
					ArtifactId = -1,
					ArtifactTypeId = sourceWorkspaceDescriptorArtifactTypeId,
					Name = Utils.GetFormatForWorkspaceOrJobDisplay(currentInstanceName, workspaceDto.Name, sourceWorkspaceArtifactId),
					SourceCaseArtifactId = sourceWorkspaceArtifactId,
					SourceCaseName = workspaceDto.Name,
					SourceInstanceName = currentInstanceName
				};

				int artifactId = sourceWorkspaceRepository.Create(sourceWorkspaceDto);
				sourceWorkspaceDto.ArtifactId = artifactId;
			}

			sourceWorkspaceDto.ArtifactTypeId = sourceWorkspaceDescriptorArtifactTypeId;

			// Check to see if instance should be updated
			if (sourceWorkspaceDto.SourceCaseName != workspaceDto.Name || sourceWorkspaceDto.SourceInstanceName != currentInstanceName)
			{
				sourceWorkspaceDto.Name = Utils.GetFormatForWorkspaceOrJobDisplay(currentInstanceName, workspaceDto.Name, sourceWorkspaceArtifactId);
				sourceWorkspaceDto.SourceCaseName = workspaceDto.Name;
				sourceWorkspaceDto.SourceInstanceName = currentInstanceName;
				sourceWorkspaceRepository.Update(sourceWorkspaceDto);
			}

			return sourceWorkspaceDto;
		}
	}
}