using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class TargetWorkspaceJobHistoryManager : ITargetWorkspaceJobHistoryManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public TargetWorkspaceJobHistoryManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public TargetWorkspaceJobHistoryDTO InitializeWorkspace(
			int sourceWorkspaceArtifactId,
			int destinationWorkspaceArtifactId,
			int sourceWorkspaceArtifactTypeId,
			int sourceWorkspaceRDOInstanceArtifactId,
			int jobHistoryArtifactId)
		{
			// Set up repositories
			ITargetWorkspaceJobHistoryRepository targetWorkspaceJobHistoryRepository = _repositoryFactory.GetTargetWorkspaceJobHistoryRepository(destinationWorkspaceArtifactId);
			ISourceWorkspaceJobHistoryRepository sourceWorkspaceJobHistoryRepository = _repositoryFactory.GetSourceWorkspaceJobHistoryRepository(sourceWorkspaceArtifactId);

			// Create object type if it does not exist
			int? jobHistoryArtifactTypeId = targetWorkspaceJobHistoryRepository.RetrieveObjectTypeDescriptorArtifactTypeId();
			if (!jobHistoryArtifactTypeId.HasValue)
			{
				jobHistoryArtifactTypeId = targetWorkspaceJobHistoryRepository.CreateObjectType(sourceWorkspaceArtifactTypeId);
			}

			// Create Job History fields if they do not exist
			if (!targetWorkspaceJobHistoryRepository.ObjectTypeFieldsExist(jobHistoryArtifactTypeId.Value))
			{
				targetWorkspaceJobHistoryRepository.CreateObjectTypeFields(jobHistoryArtifactTypeId.Value);
			}

			// Create fields on document if they do not exist
			if (!targetWorkspaceJobHistoryRepository.JobHistoryFieldExistsOnDocument(jobHistoryArtifactTypeId.Value))
			{
				targetWorkspaceJobHistoryRepository.CreateJobHistoryFieldOnDocument(jobHistoryArtifactTypeId.Value);
			}

			// Create instance of Job History object
			SourceWorkspaceJobHistoryDTO sourceWorkspaceJobHistoryDto = sourceWorkspaceJobHistoryRepository.Retrieve(jobHistoryArtifactId);
			var jobHistoryDto = new TargetWorkspaceJobHistoryDTO()
			{
				Name = String.Format("{0} - {1}", sourceWorkspaceJobHistoryDto.Name, jobHistoryArtifactId),
				SourceWorkspaceArtifactId = sourceWorkspaceRDOInstanceArtifactId,
				JobHistoryArtifactId = jobHistoryArtifactId,
				JobHistoryName = sourceWorkspaceJobHistoryDto.Name,
			};

			int artifactId = targetWorkspaceJobHistoryRepository.Create(jobHistoryArtifactTypeId.Value,
				jobHistoryDto);

			jobHistoryDto.ArtifactId = artifactId;

			return jobHistoryDto;
		}
	}
}