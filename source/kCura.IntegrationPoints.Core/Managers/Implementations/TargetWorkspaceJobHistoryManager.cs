using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class TargetWorkspaceJobHistoryManager : ITargetWorkspaceJobHistoryManager
	{
		private readonly ITargetWorkspaceJobHistoryRepository _targetWorkspaceJobHistoryRepository;
		private readonly ISourceWorkspaceJobHistoryRepository _sourceWorkspaceJobHistoryRepository;

		public TargetWorkspaceJobHistoryManager(
			ITargetWorkspaceJobHistoryRepository targetWorkspaceJobHistoryRepository,
			ISourceWorkspaceJobHistoryRepository sourceWorkspaceJobHistoryRepository)
		{
			_targetWorkspaceJobHistoryRepository = targetWorkspaceJobHistoryRepository;
			_sourceWorkspaceJobHistoryRepository = sourceWorkspaceJobHistoryRepository;
		}

		public TargetWorkspaceJobHistoryDTO InitializeWorkspace(
			int sourceWorkspaceArtifactId, 
			int destinationWorkspaceArtifactId, 
			int sourceWorkspaceArtifactTypeId,
			int sourceWorkspaceRDOInstanceArtifactId,
			int jobHistoryArtifactId)
		{
			// Create object type if it does not exist
			int? jobHistoryArtifactTypeId =
				_targetWorkspaceJobHistoryRepository.RetrieveObjectTypeDescriptorArtifactTypeId(destinationWorkspaceArtifactId);
			if (!jobHistoryArtifactTypeId.HasValue)
			{
				jobHistoryArtifactTypeId = _targetWorkspaceJobHistoryRepository.CreateObjectType(destinationWorkspaceArtifactId, sourceWorkspaceArtifactTypeId);
			}

			// Create Job History fields if they do not exist
			if (!_targetWorkspaceJobHistoryRepository.ObjectTypeFieldsExist(destinationWorkspaceArtifactId,
					jobHistoryArtifactTypeId.Value))
			{
				_targetWorkspaceJobHistoryRepository.CreateObjectTypeFields(destinationWorkspaceArtifactId, jobHistoryArtifactTypeId.Value);
			}

			// Create fields on document if they do not exist
			if (!_targetWorkspaceJobHistoryRepository.JobHistoryFieldExistsOnDocument(destinationWorkspaceArtifactId, jobHistoryArtifactTypeId.Value))
			{
				_targetWorkspaceJobHistoryRepository.CreateJobHistoryFieldOnDocument(destinationWorkspaceArtifactId, jobHistoryArtifactTypeId.Value);
			}

			// Create instance of Job History object
			SourceWorkspaceJobHistoryDTO sourceWorkspaceJobHistoryDto = _sourceWorkspaceJobHistoryRepository.Retrieve(sourceWorkspaceArtifactId, jobHistoryArtifactId);
			var jobHistoryDto = new TargetWorkspaceJobHistoryDTO()
			{
				Name = String.Format("{0} - {1}", sourceWorkspaceJobHistoryDto.Name, jobHistoryArtifactId),
				SourceWorkspaceArtifactId = sourceWorkspaceRDOInstanceArtifactId,
				JobHistoryArtifactId = jobHistoryArtifactId,
				JobHistoryName = sourceWorkspaceJobHistoryDto.Name,
			};

			int artifactId = _targetWorkspaceJobHistoryRepository.Create(destinationWorkspaceArtifactId, jobHistoryArtifactTypeId.Value,
				jobHistoryDto);

			jobHistoryDto.ArtifactId = artifactId;

			return jobHistoryDto;
		}
	}
}