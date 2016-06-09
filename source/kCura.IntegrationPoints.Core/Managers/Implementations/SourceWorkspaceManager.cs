using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceWorkspaceManager : ISourceWorkspaceManager
	{
		private const string _ERROR_MESSAGE =
			"Unable to create source workspace and job fields in the destination workspace. Please contact your system administrator.";

		private readonly IRepositoryFactory _repositoryFactory;

		public SourceWorkspaceManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public SourceWorkspaceDTO InitializeWorkspace(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId)
		{
			ISourceWorkspaceRepository sourceWorkspaceRepository = _repositoryFactory.GetSourceWorkspaceRepository(destinationWorkspaceArtifactId);
			IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(destinationWorkspaceArtifactId);

			int sourceWorkspaceDescriptorArtifactTypeId = CreateSourceWorkspaceObjectType(destinationWorkspaceArtifactId,
				sourceWorkspaceRepository, artifactGuidRepository);

			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(destinationWorkspaceArtifactId);

			CreateSourceWorkspaceFields(artifactGuidRepository, fieldRepository, sourceWorkspaceRepository, sourceWorkspaceDescriptorArtifactTypeId);
			CreateDocumentFields(sourceWorkspaceDescriptorArtifactTypeId, artifactGuidRepository, sourceWorkspaceRepository, fieldRepository);

			SourceWorkspaceDTO sourceWorkspaceDto = CreateSourceWorkspaceDto(sourceWorkspaceArtifactId, sourceWorkspaceDescriptorArtifactTypeId, sourceWorkspaceRepository);
			return sourceWorkspaceDto;
		}

		private int CreateSourceWorkspaceObjectType(int workspaceArtifactId,
			ISourceWorkspaceRepository sourceWorkspaceRepository, IArtifactGuidRepository artifactGuidRepository)
		{
			IObjectTypeRepository objectTypeRepository = _repositoryFactory.GetObjectTypeRepository(workspaceArtifactId);

			// Check workspace for instance of the object type GUID
			int sourceWorkspaceDescriptorArtifactTypeId;
			try
			{
				sourceWorkspaceDescriptorArtifactTypeId =
					objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceWorkspaceDTO.ObjectTypeGuid);
			}
			catch (TypeLoadException)
			{
				// GUID doesn't exist in the workspace, so we try to see if the field name exists and assign a GUID to the field
				int? sourceWorkspaceObjectTypeArtifactId =
					objectTypeRepository.RetrieveObjectTypeArtifactId(
						IntegrationPoints.Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME);

				sourceWorkspaceObjectTypeArtifactId = sourceWorkspaceObjectTypeArtifactId ?? sourceWorkspaceRepository.CreateObjectType();

				// Associate a GUID with the newly created or existing object type
				try
				{
					artifactGuidRepository.InsertArtifactGuidForArtifactId(sourceWorkspaceObjectTypeArtifactId.Value, SourceWorkspaceDTO.ObjectTypeGuid);
				}
				catch (Exception e)
				{
					objectTypeRepository.Delete(sourceWorkspaceObjectTypeArtifactId.Value);
					throw new Exception(_ERROR_MESSAGE, e);
				}

				// Get descriptor artifact type id of the now existing object type
				sourceWorkspaceDescriptorArtifactTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceWorkspaceDTO.ObjectTypeGuid);

				// Delete the tab if it exists (it should always exist since we're creating the object type one line above)
				ITabRepository tabRepository = _repositoryFactory.GetTabRepository(workspaceArtifactId);
				int? sourceWorkspaceTabId = tabRepository.RetrieveTabArtifactId(
					sourceWorkspaceDescriptorArtifactTypeId, IntegrationPoints.Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME);
				if (sourceWorkspaceTabId.HasValue)
				{
					tabRepository.Delete(sourceWorkspaceTabId.Value);
				}
			}

			return sourceWorkspaceDescriptorArtifactTypeId;
		}

		private void CreateSourceWorkspaceFields(IArtifactGuidRepository artifactGuidRepository,
			IFieldRepository fieldRepository, ISourceWorkspaceRepository sourceWorkspaceRepository,
			int sourceWorkspaceDescriptorArtifactTypeId)
		{
			var fieldGuids = new List<Guid>(2) { SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid, SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid };

			IDictionary<Guid, bool> objectTypeFields = artifactGuidRepository.GuidsExist(fieldGuids);

			IList<Guid> missingFieldGuids = objectTypeFields.Where(x => x.Value == false).Select(y => y.Key).ToList();

			if (missingFieldGuids.Any())
			{
				IDictionary<Guid, int> guidToArtifactId = new Dictionary<Guid, int>();
				
				if (missingFieldGuids.Contains(SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid))
				{
					int? artifactId =
						fieldRepository.RetrieveField(IntegrationPoints.Contracts.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME,
							sourceWorkspaceDescriptorArtifactTypeId, (int)Relativity.Client.FieldType.WholeNumber);
					if (artifactId.HasValue)
					{
						guidToArtifactId.Add(SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid, artifactId.Value);
						missingFieldGuids.Remove(SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid);
					}
				}
				if (missingFieldGuids.Contains(SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid))
				{
					int? artifactId =
						fieldRepository.RetrieveField(IntegrationPoints.Contracts.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME,
							sourceWorkspaceDescriptorArtifactTypeId, (int)Relativity.Client.FieldType.FixedLengthText);
					if (artifactId.HasValue)
					{
						guidToArtifactId.Add(SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid, artifactId.Value);
						missingFieldGuids.Remove(SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid);
					}
				}

				if (missingFieldGuids.Any())
				{
					IDictionary<Guid, int> missingGuidToArtifactId =
						sourceWorkspaceRepository.CreateObjectTypeFields(sourceWorkspaceDescriptorArtifactTypeId, missingFieldGuids);

					guidToArtifactId = guidToArtifactId.Union(missingGuidToArtifactId).ToDictionary(k => k.Key, v => v.Value);
				}

				try
				{
					artifactGuidRepository.InsertArtifactGuidsForArtifactIds(guidToArtifactId);
				}
				catch (Exception e)
				{
					fieldRepository.Delete(guidToArtifactId.Values);
					throw new Exception(_ERROR_MESSAGE, e);
				}
			}
		}

		private void CreateDocumentFields(int sourceWorkspaceDescriptorArtifactTypeId, IArtifactGuidRepository artifactGuidRepository,
			ISourceWorkspaceRepository sourceWorkspaceRepository, IFieldRepository fieldRepository)
		{
			bool sourceWorkspaceFieldOnDocumentExists = artifactGuidRepository.GuidExists(SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid);
			if (!sourceWorkspaceFieldOnDocumentExists)
			{
				int? fieldArtifactId =
						fieldRepository.RetrieveField(IntegrationPoints.Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
							(int)Relativity.Client.ArtifactType.Document, (int)Relativity.Client.FieldType.MultipleObject);

				int sourceWorkspaceFieldArtifactId = fieldArtifactId ?? sourceWorkspaceRepository.CreateSourceWorkspaceFieldOnDocument(sourceWorkspaceDescriptorArtifactTypeId);

				// Set the filter type
				try
				{
					int? retrieveArtifactViewFieldId = fieldRepository.RetrieveArtifactViewFieldId(sourceWorkspaceFieldArtifactId);
					if (!retrieveArtifactViewFieldId.HasValue)
					{
						throw new Exception(_ERROR_MESSAGE);
					}

					fieldRepository.UpdateFilterType(retrieveArtifactViewFieldId.Value, "Popup");
				}
				catch (Exception e)
				{
					fieldRepository.Delete(new[] { sourceWorkspaceFieldArtifactId });
					throw new Exception(_ERROR_MESSAGE, e);
				}

				// Set the overlay behavior
				try
				{
					fieldRepository.SetOverlayBehavior(sourceWorkspaceFieldArtifactId, true);
				}
				catch (Exception e)
				{
					fieldRepository.Delete(new[] { sourceWorkspaceFieldArtifactId });
					throw new Exception(_ERROR_MESSAGE, e);
				}

				// Set the field artifact guid
				try
				{
					artifactGuidRepository.InsertArtifactGuidForArtifactId(sourceWorkspaceFieldArtifactId,
						SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid);
				}
				catch (Exception e)
				{
					fieldRepository.Delete(new[] { sourceWorkspaceFieldArtifactId });
					throw new Exception(_ERROR_MESSAGE, e);
				}
			}
		}
		
		private SourceWorkspaceDTO CreateSourceWorkspaceDto(int workspaceArtifactId,
			int sourceWorkspaceDescriptorArtifactTypeId, ISourceWorkspaceRepository sourceWorkspaceRepository)
		{
			IWorkspaceRepository workspaceRepository = _repositoryFactory.GetWorkspaceRepository();
			WorkspaceDTO workspaceDto = workspaceRepository.Retrieve(workspaceArtifactId);
			SourceWorkspaceDTO sourceWorkspaceDto = sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(workspaceArtifactId);

			if (sourceWorkspaceDto == null)
			{
				sourceWorkspaceDto = new SourceWorkspaceDTO
				{
					ArtifactId = -1,
					Name = Utils.GetFormatForWorkspaceOrJobDisplay(workspaceDto.Name, workspaceArtifactId),
					SourceCaseArtifactId = workspaceArtifactId,
					SourceCaseName = workspaceDto.Name
				};

				int artifactId = sourceWorkspaceRepository.Create(sourceWorkspaceDescriptorArtifactTypeId, sourceWorkspaceDto);
				sourceWorkspaceDto.ArtifactId = artifactId;
			}

			sourceWorkspaceDto.ArtifactTypeId = sourceWorkspaceDescriptorArtifactTypeId;

			// Check to see if instance should be updated
			if (sourceWorkspaceDto.SourceCaseName != workspaceDto.Name)
			{
				sourceWorkspaceDto.Name = Utils.GetFormatForWorkspaceOrJobDisplay(workspaceDto.Name, workspaceArtifactId);
				sourceWorkspaceDto.SourceCaseName = workspaceDto.Name;
				sourceWorkspaceRepository.Update(sourceWorkspaceDto);
			}

			return sourceWorkspaceDto;
		}
	}
}