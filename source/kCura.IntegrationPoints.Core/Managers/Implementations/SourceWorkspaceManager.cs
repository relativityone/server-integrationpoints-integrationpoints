﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Data.QueryGenerator;

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
			IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(destinationWorkspaceArtifactId);
			IObjectTypeRepository objectTypeRepository = _repositoryFactory.GetObjectTypeRepository(destinationWorkspaceArtifactId);
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(destinationWorkspaceArtifactId);

			// Create object type if it does not exist
			int? sourceWorkspaceDescriptorArtifactTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceWorkspaceDTO.ObjectTypeGuid);
			if (!sourceWorkspaceDescriptorArtifactTypeId.HasValue)
			{
				int sourceWorkspaceArtifactTypeId = sourceWorkspaceRepository.CreateObjectType();

				// Insert entry to the ArtifactGuid table for new object type
				try
				{
					artifactGuidRepository.InsertArtifactGuidForArtifactId(sourceWorkspaceArtifactTypeId, SourceWorkspaceDTO.ObjectTypeGuid);
				}
				catch (Exception e)
				{
					objectTypeRepository.Delete(sourceWorkspaceArtifactTypeId);
					throw new Exception("Unable to create Source Workspace object type: Unable to associate new object type with Artifact Guid", e);	
				}

				// Get descriptor id
				sourceWorkspaceDescriptorArtifactTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceWorkspaceDTO.ObjectTypeGuid);

				// Delete the tab if it exists (it should always exist since we're creating the object type one line above)
				ITabRepository tabRepository = _repositoryFactory.GetTabRepository(destinationWorkspaceArtifactId);
				int? sourceWorkspaceTabId = tabRepository.RetrieveTabArtifactId(sourceWorkspaceDescriptorArtifactTypeId.Value, IntegrationPoints.Contracts.Constants.SOURCEWORKSPACE_NAME_FIELD_NAME);
				if (sourceWorkspaceTabId.HasValue)
				{
					tabRepository.Delete(sourceWorkspaceTabId.Value);
				}
			}

			// Create Source Workspace fields if they do not exist
			IDictionary<Guid, bool> objectTypeFields = artifactGuidRepository.GuidsExist(new []
			{
				SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid, SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid
			});
			IList<Guid> missingFieldGuids = objectTypeFields.Where(x => x.Value == false).Select(y => y.Key).ToList();
			if (missingFieldGuids.Any())
			{
				IDictionary<Guid, int> guidToIdDictionary = sourceWorkspaceRepository.CreateObjectTypeFields(sourceWorkspaceDescriptorArtifactTypeId.Value, missingFieldGuids);	

				try
				{
					artifactGuidRepository.InsertArtifactGuidsForArtifactIds(guidToIdDictionary);
				}
				catch (Exception e)
				{
					fieldRepository.Delete(guidToIdDictionary.Values);
					throw new Exception("Unable to create Source Workspace fields: Unable to associate new fields with Artifact Guids", e);	
				}
			}

			// Create fields on document if they do not exist
			bool sourceWorkspaceFieldOnDocumentExists =
				artifactGuidRepository.GuidExists(SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid);
			if (!sourceWorkspaceFieldOnDocumentExists)
			{
				int fieldArtifactId = sourceWorkspaceRepository.CreateSourceWorkspaceFieldOnDocument(sourceWorkspaceDescriptorArtifactTypeId.Value);

				try
				{
					artifactGuidRepository.InsertArtifactGuidForArtifactId(fieldArtifactId,
						SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid);
				}
				catch (Exception e)
				{
					fieldRepository.Delete(new[] { fieldArtifactId });
					throw new Exception("Unable to create Source Workspace multi-object field on Document: Unable to associate new Artifact Guids", e);
				}

				try
				{
					fieldRepository.SetOverlayBehavior(fieldArtifactId, true);
				}
				catch (Exception e)
				{
					fieldRepository.Delete(new[] { fieldArtifactId });
					throw new Exception("Unable to create Source Workspace multi-object field on Document: Unable to set the default Overlay Behavior", e);
				}
			}

			// Get or create instance of Source Workspace object
			WorkspaceDTO workspaceDto = workspaceRepository.Retrieve(sourceWorkspaceArtifactId);
			SourceWorkspaceDTO sourceWorkspaceDto = sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(sourceWorkspaceArtifactId);
			if (sourceWorkspaceDto == null)
			{
				sourceWorkspaceDto = new SourceWorkspaceDTO()
				{
					ArtifactId = -1,
					Name = $"{workspaceDto.Name} - {sourceWorkspaceArtifactId}",
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
				sourceWorkspaceDto.Name = $"{workspaceDto.Name} - {sourceWorkspaceArtifactId}";
				sourceWorkspaceDto.SourceCaseName = workspaceDto.Name;
				sourceWorkspaceRepository.Update(sourceWorkspaceDto);
			}

			return sourceWorkspaceDto;
		}
	}
}