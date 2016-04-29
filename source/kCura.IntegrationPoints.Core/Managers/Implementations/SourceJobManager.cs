using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceJobManager : ISourceJobManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public SourceJobManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public SourceJobDTO InitializeWorkspace(
			int sourceWorkspaceArtifactId,
			int destinationWorkspaceArtifactId,
			int sourceWorkspaceArtifactTypeId,
			int sourceWorkspaceRDOInstanceArtifactId,
			int jobHistoryArtifactId)
		{
			// Set up repositories
			ISourceJobRepository sourceJobRepository = _repositoryFactory.GetSourceJobRepository(destinationWorkspaceArtifactId);
			ISourceWorkspaceJobHistoryRepository sourceWorkspaceJobHistoryRepository = _repositoryFactory.GetSourceWorkspaceJobHistoryRepository(sourceWorkspaceArtifactId);
			IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(destinationWorkspaceArtifactId);
			IObjectTypeRepository objectTypeRepository = _repositoryFactory.GetObjectTypeRepository(destinationWorkspaceArtifactId);
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(destinationWorkspaceArtifactId);

			// Create object type if it does not exist
			int? sourceJobDescriptorArtifactTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceJobDTO.ObjectTypeGuid);
			if (!sourceJobDescriptorArtifactTypeId.HasValue)
			{
				int sourceJobArtifactTypeId = sourceJobRepository.CreateObjectType(sourceWorkspaceArtifactTypeId);

				// Insert entry to the ArtifactGuid table for new object type
				try
				{
					artifactGuidRepository.InsertArtifactGuidForArtifactId(sourceJobArtifactTypeId, SourceJobDTO.ObjectTypeGuid);
				}
				catch (Exception e)
				{
					objectTypeRepository.Delete(sourceJobArtifactTypeId);
					throw new Exception("Unable to create Source Job object type: Unable to associate new object type with Artifact Guid", e);	
				}

				// Get the descriptor id
				sourceJobDescriptorArtifactTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(SourceJobDTO.ObjectTypeGuid);
			}

			// Create Job History fields if they do not exist
			IDictionary<Guid, bool> objectTypeFields = artifactGuidRepository.GuidsExist(new[]
			{
				SourceJobDTO.Fields.JobHistoryIdFieldGuid, SourceJobDTO.Fields.JobHistoryNameFieldGuid
			});
			IList<Guid> missingFieldGuids = objectTypeFields.Where(x => x.Value == false).Select(y => y.Key).ToList();
			if (missingFieldGuids.Any())
			{
				IDictionary<Guid, int> guidToIdDictionary =
					sourceJobRepository.CreateObjectTypeFields(sourceJobDescriptorArtifactTypeId.Value, missingFieldGuids);

				try
				{
					artifactGuidRepository.InsertArtifactGuidsForArtifactIds(guidToIdDictionary);
				}
				catch (Exception e)
				{
					fieldRepository.Delete(guidToIdDictionary.Values);
					throw new Exception("Unable to create Source Job fields: Unable to associate new fields with Artifact Guids", e);
				}
			}

			// Create fields on document if they do not exist
			bool jobHistoryFieldOnDocumentExists =
				artifactGuidRepository.GuidExists(SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid);
			if (!jobHistoryFieldOnDocumentExists)
			{
				int fieldArtifactId = sourceJobRepository.CreateSourceJobFieldOnDocument(sourceJobDescriptorArtifactTypeId.Value);

				try
				{
					int? retrieveArtifactViewFieldId = fieldRepository.RetrieveArtifactViewFieldId(fieldArtifactId);
					if (!retrieveArtifactViewFieldId.HasValue)
					{
						throw new Exception("Unable to retrieve artifact view field id for field");
					}

					fieldRepository.UpdateFilterType(retrieveArtifactViewFieldId.Value, "Popup");
				}
				catch (Exception e)
				{
					fieldRepository.Delete(new[] { fieldArtifactId });
					throw new Exception("Unable to create Source Job multi-object field on Document" + e);	
				}

				try
				{
					fieldRepository.SetOverlayBehavior(fieldArtifactId, true);
				}
				catch (Exception e)
				{
					fieldRepository.Delete(new[] { fieldArtifactId });
					throw new Exception("Unable to create Source Job multi-object field on Document: Unable to set the default Overlay Behavior", e);
				}

				try
				{
					artifactGuidRepository.InsertArtifactGuidForArtifactId(fieldArtifactId, SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid);
				}
				catch (Exception e)
				{
					fieldRepository.Delete(new[] { fieldArtifactId });
					throw new Exception("Unable to create Source Job multi-object field on Document: Unable to associate new Artifact Guids", e);
				}
			}

			// Create instance of Job History object
			SourceWorkspaceJobHistoryDTO sourceWorkspaceJobHistoryDto = sourceWorkspaceJobHistoryRepository.Retrieve(jobHistoryArtifactId);
			var jobHistoryDto = new SourceJobDTO()
			{
				Name = Utils.GetFormatForWorkspaceOrJobDisplay(sourceWorkspaceJobHistoryDto.Name, jobHistoryArtifactId),
				SourceWorkspaceArtifactId = sourceWorkspaceRDOInstanceArtifactId,
				JobHistoryArtifactId = jobHistoryArtifactId,
				JobHistoryName = sourceWorkspaceJobHistoryDto.Name,
			};

			int artifactId = sourceJobRepository.Create(sourceJobDescriptorArtifactTypeId.Value,
				jobHistoryDto);

			jobHistoryDto.ArtifactId = artifactId;

			return jobHistoryDto;
		}
	}
}