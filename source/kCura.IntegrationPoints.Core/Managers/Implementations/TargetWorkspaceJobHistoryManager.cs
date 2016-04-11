using System;
using System.Collections.Generic;
using System.Linq;
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
			IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(destinationWorkspaceArtifactId);
			IObjectTypeRepository objectTypeRepository = _repositoryFactory.GetObjectTypeRepository(destinationWorkspaceArtifactId);
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(destinationWorkspaceArtifactId);

			// Create object type if it does not exist
			int? jobHistoryDescriptorArtifactTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(TargetWorkspaceJobHistoryDTO.ObjectTypeGuid);
			if (!jobHistoryDescriptorArtifactTypeId.HasValue)
			{
				int jobHistoryArtifactTypeId = targetWorkspaceJobHistoryRepository.CreateObjectType(sourceWorkspaceArtifactTypeId);

				// Insert entry to the ArtifactGuid table for new object type
				try
				{
					artifactGuidRepository.InsertArtifactGuidForArtifactId(jobHistoryArtifactTypeId, TargetWorkspaceJobHistoryDTO.ObjectTypeGuid);
				}
				catch (Exception e)
				{
					objectTypeRepository.Delete(jobHistoryArtifactId);
					throw new Exception("Unable to create Source Job object type: Unable to associate new object type with Artifact Guid", e);	
				}

				// Get the descriptor id
				jobHistoryDescriptorArtifactTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(TargetWorkspaceJobHistoryDTO.ObjectTypeGuid);
			}

			// Create Job History fields if they do not exist
			IDictionary<Guid, bool> objectTypeFields = artifactGuidRepository.GuidsExist(new[]
			{
				TargetWorkspaceJobHistoryDTO.Fields.JobHistoryIdFieldGuid, TargetWorkspaceJobHistoryDTO.Fields.JobHistoryNameFieldGuid
			});
			IList<Guid> missingFieldGuids = objectTypeFields.Where(x => x.Value == false).Select(y => y.Key).ToList();
			if (missingFieldGuids.Any())
			{
				IDictionary<Guid, int> guidToIdDictionary =
					targetWorkspaceJobHistoryRepository.CreateObjectTypeFields(jobHistoryDescriptorArtifactTypeId.Value, missingFieldGuids);

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
				artifactGuidRepository.GuidExists(TargetWorkspaceJobHistoryDTO.Fields.JobHistoryFieldOnDocumentGuid);
			if (!jobHistoryFieldOnDocumentExists)
			{
				int fieldArtifactId = targetWorkspaceJobHistoryRepository.CreateJobHistoryFieldOnDocument(jobHistoryDescriptorArtifactTypeId.Value);

				try
				{
					artifactGuidRepository.InsertArtifactGuidForArtifactId(fieldArtifactId, TargetWorkspaceJobHistoryDTO.Fields.JobHistoryFieldOnDocumentGuid);
				}
				catch (Exception e)
				{
					fieldRepository.Delete(new[] { fieldArtifactId });
					throw new Exception("Unable to create Source Job multi-object field on Document: Unable to associate new Artifact Guids", e);
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
			}

			// Create instance of Job History object
			SourceWorkspaceJobHistoryDTO sourceWorkspaceJobHistoryDto = sourceWorkspaceJobHistoryRepository.Retrieve(jobHistoryArtifactId);
			var jobHistoryDto = new TargetWorkspaceJobHistoryDTO()
			{
				Name = $"{sourceWorkspaceJobHistoryDto.Name} - {jobHistoryArtifactId}",
				SourceWorkspaceArtifactId = sourceWorkspaceRDOInstanceArtifactId,
				JobHistoryArtifactId = jobHistoryArtifactId,
				JobHistoryName = sourceWorkspaceJobHistoryDto.Name,
			};

			int artifactId = targetWorkspaceJobHistoryRepository.Create(jobHistoryDescriptorArtifactTypeId.Value,
				jobHistoryDto);

			jobHistoryDto.ArtifactId = artifactId;

			return jobHistoryDto;
		}
	}
}