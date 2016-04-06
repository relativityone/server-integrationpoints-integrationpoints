using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceWorkspaceManager : ISourceWorkspaceManager
	{
		private readonly ISourceWorkspaceRepository _sourceWorkspaceRepository;
		private readonly IWorkspaceRepository _workspaceRepository;

		public SourceWorkspaceManager(ISourceWorkspaceRepository sourceWorkspaceRepository, IWorkspaceRepository workspaceRepository)
		{
			_sourceWorkspaceRepository = sourceWorkspaceRepository;
			_workspaceRepository = workspaceRepository;
		}

		public SourceWorkspaceDTO InitializeWorkspace(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId)
		{
			// Create object type if it does not exist
			int? sourceWorkspaceArtifactTypeId = _sourceWorkspaceRepository.RetrieveObjectTypeDescriptorArtifactTypeId(destinationWorkspaceArtifactId);
			if (!sourceWorkspaceArtifactTypeId.HasValue)
			{
				sourceWorkspaceArtifactTypeId = _sourceWorkspaceRepository.CreateObjectType(destinationWorkspaceArtifactId);	
			}

			// Create Source Workspace fields if they do not exist
			if (!_sourceWorkspaceRepository.ObjectTypeFieldsExist(destinationWorkspaceArtifactId,
				sourceWorkspaceArtifactTypeId.Value))
			{
				_sourceWorkspaceRepository.CreateObjectTypeFields(destinationWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value);	
			}

			// Create fields on document if they do not exist
			if (!_sourceWorkspaceRepository.SourceWorkspaceFieldExistsOnDocument(destinationWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value))
			{
				_sourceWorkspaceRepository.CreateSourceWorkspaceFieldOnDocument(destinationWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value);
			}

			// Get or create instance of Source Workspace object
			WorkspaceDTO workspaceDto = _workspaceRepository.Retrieve(sourceWorkspaceArtifactId);
			SourceWorkspaceDTO sourceWorkspaceDto = _sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(destinationWorkspaceArtifactId, sourceWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value);
			if (sourceWorkspaceDto == null)
			{
				sourceWorkspaceDto = new SourceWorkspaceDTO()
				{
					ArtifactId = -1,
					Name = String.Format("{0} - {1}", workspaceDto.Name, sourceWorkspaceArtifactId),
					SourceCaseArtifactId = sourceWorkspaceArtifactId,
					SourceCaseName = workspaceDto.Name
				};
				int artifactId = _sourceWorkspaceRepository.Create(sourceWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value, sourceWorkspaceDto);

				sourceWorkspaceDto.ArtifactId = artifactId;
			}

			sourceWorkspaceDto.ArtifactTypeId = sourceWorkspaceArtifactTypeId.Value;

			// Check to see if instance should be updated
			if (sourceWorkspaceDto.SourceCaseName != workspaceDto.Name)
			{
				sourceWorkspaceDto.Name = String.Format("{0} - {1}", workspaceDto.Name, sourceWorkspaceArtifactId);
				sourceWorkspaceDto.SourceCaseName = workspaceDto.Name;
				_sourceWorkspaceRepository.Update(destinationWorkspaceArtifactId, sourceWorkspaceDto);
			}

			return sourceWorkspaceDto;
		}
	}
}