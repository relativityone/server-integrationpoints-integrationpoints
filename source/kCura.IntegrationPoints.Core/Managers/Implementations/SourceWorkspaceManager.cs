using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceWorkspaceManager : ISourceWorkspaceManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public SourceWorkspaceManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public SourceWorkspaceDTO InitializeWorkspace(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId)
		{
			// Set up repositories
			ISourceWorkspaceRepository sourceWorkspaceRepository = _repositoryFactory.GetSourceWorkspaceRepository(destinationWorkspaceArtifactId);
			IWorkspaceRepository workspaceRepository = _repositoryFactory.GetWorkspaceRepository();

			// Create object type if it does not exist
			int? sourceWorkspaceArtifactTypeId = sourceWorkspaceRepository.RetrieveObjectTypeDescriptorArtifactTypeId();
			if (!sourceWorkspaceArtifactTypeId.HasValue)
			{
				sourceWorkspaceArtifactTypeId = sourceWorkspaceRepository.CreateObjectType();	
			}

			// Create Source Workspace fields if they do not exist
			if (!sourceWorkspaceRepository.ObjectTypeFieldsExist(sourceWorkspaceArtifactTypeId.Value))
			{
				sourceWorkspaceRepository.CreateObjectTypeFields(sourceWorkspaceArtifactTypeId.Value);	
			}

			// Create fields on document if they do not exist
			if (!sourceWorkspaceRepository.SourceWorkspaceFieldExistsOnDocument(sourceWorkspaceArtifactTypeId.Value))
			{
				sourceWorkspaceRepository.CreateSourceWorkspaceFieldOnDocument(sourceWorkspaceArtifactTypeId.Value);
			}

			// Get or create instance of Source Workspace object
			WorkspaceDTO workspaceDto = workspaceRepository.Retrieve(sourceWorkspaceArtifactId);
			SourceWorkspaceDTO sourceWorkspaceDto = sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(sourceWorkspaceArtifactId, sourceWorkspaceArtifactTypeId.Value);
			if (sourceWorkspaceDto == null)
			{
				sourceWorkspaceDto = new SourceWorkspaceDTO()
				{
					ArtifactId = -1,
					Name = String.Format("{0} - {1}", workspaceDto.Name, sourceWorkspaceArtifactId),
					SourceCaseArtifactId = sourceWorkspaceArtifactId,
					SourceCaseName = workspaceDto.Name
				};
				int artifactId = sourceWorkspaceRepository.Create(sourceWorkspaceArtifactTypeId.Value, sourceWorkspaceDto);

				sourceWorkspaceDto.ArtifactId = artifactId;
			}

			sourceWorkspaceDto.ArtifactTypeId = sourceWorkspaceArtifactTypeId.Value;

			// Check to see if instance should be updated
			if (sourceWorkspaceDto.SourceCaseName != workspaceDto.Name)
			{
				sourceWorkspaceDto.Name = String.Format("{0} - {1}", workspaceDto.Name, sourceWorkspaceArtifactId);
				sourceWorkspaceDto.SourceCaseName = workspaceDto.Name;
				sourceWorkspaceRepository.Update(sourceWorkspaceDto);
			}

			return sourceWorkspaceDto;
		}
	}
}