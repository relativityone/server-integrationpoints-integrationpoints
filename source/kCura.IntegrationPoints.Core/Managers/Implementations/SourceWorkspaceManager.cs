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
			int? sourceWorkspaceDescriptorArtifactTypeId = sourceWorkspaceRepository.RetrieveObjectTypeDescriptorArtifactTypeId();
			if (!sourceWorkspaceDescriptorArtifactTypeId.HasValue)
			{
				int sourceWorkspaceArtifactTypeId = sourceWorkspaceRepository.CreateObjectType();

				// Insert entry to the ArtifactGuid table for new object type
				IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(destinationWorkspaceArtifactId);
				artifactGuidRepository.InsertArtifactGuidForArtifactId(sourceWorkspaceArtifactTypeId, SourceWorkspaceDTO.ObjectTypeGuid);

				// Get descriptor id
				sourceWorkspaceDescriptorArtifactTypeId = sourceWorkspaceRepository.RetrieveObjectTypeDescriptorArtifactTypeId();

				// Delete the tab if it exists (it should always exist since we're creating the object type one line above)
				int? sourceWorkspaceTabId = sourceWorkspaceRepository.RetrieveTabArtifactId(sourceWorkspaceDescriptorArtifactTypeId.Value);
				if (sourceWorkspaceTabId.HasValue)
				{
					sourceWorkspaceRepository.DeleteTab(sourceWorkspaceTabId.Value);
				}
			}

			// Create Source Workspace fields if they do not exist
			if (!sourceWorkspaceRepository.ObjectTypeFieldsExist(sourceWorkspaceDescriptorArtifactTypeId.Value))
			{
				sourceWorkspaceRepository.CreateObjectTypeFields(sourceWorkspaceDescriptorArtifactTypeId.Value);	
			}

			// Create fields on document if they do not exist
			if (!sourceWorkspaceRepository.SourceWorkspaceFieldExistsOnDocument(sourceWorkspaceDescriptorArtifactTypeId.Value))
			{
				sourceWorkspaceRepository.CreateSourceWorkspaceFieldOnDocument(sourceWorkspaceDescriptorArtifactTypeId.Value);
			}

			// Get or create instance of Source Workspace object
			WorkspaceDTO workspaceDto = workspaceRepository.Retrieve(sourceWorkspaceArtifactId);
			SourceWorkspaceDTO sourceWorkspaceDto = sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(sourceWorkspaceArtifactId, sourceWorkspaceDescriptorArtifactTypeId.Value);
			if (sourceWorkspaceDto == null)
			{
				sourceWorkspaceDto = new SourceWorkspaceDTO()
				{
					ArtifactId = -1,
					Name = String.Format("{0} - {1}", workspaceDto.Name, sourceWorkspaceArtifactId),
					SourceCaseArtifactId = sourceWorkspaceArtifactId,
					SourceCaseName = workspaceDto.Name
				};
				int artifactId = sourceWorkspaceRepository.Create(sourceWorkspaceDescriptorArtifactTypeId.Value, sourceWorkspaceDto);

				sourceWorkspaceDto.ArtifactId = artifactId;
			}

			sourceWorkspaceDto.ArtifactTypeId = sourceWorkspaceDescriptorArtifactTypeId.Value;

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