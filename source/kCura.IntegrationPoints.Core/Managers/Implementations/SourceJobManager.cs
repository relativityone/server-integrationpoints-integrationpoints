using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using FieldType = kCura.Relativity.Client.FieldType;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceJobManager : DestinationWorkspaceFieldManagerBase, ISourceJobManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public SourceJobManager(IRepositoryFactory repositoryFactory)
			: base(repositoryFactory,
				  IntegrationPoints.Contracts.Constants.SPECIAL_SOURCEJOB_FIELD_NAME,
				  SourceJobDTO.ObjectTypeGuid)
		{
			_repositoryFactory = repositoryFactory;
		}

		public SourceJobDTO InitializeWorkspace(
			int sourceWorkspaceArtifactId,
			int destinationWorkspaceArtifactId,
			int sourceWorkspaceArtifactTypeId,
			int sourceWorkspaceRdoInstanceArtifactId,
			int jobHistoryArtifactId)
		{
			// Set up repositories
			ISourceJobRepository sourceJobRepository = _repositoryFactory.GetSourceJobRepository(destinationWorkspaceArtifactId);
			ISourceWorkspaceJobHistoryRepository sourceWorkspaceJobHistoryRepository = _repositoryFactory.GetSourceWorkspaceJobHistoryRepository(sourceWorkspaceArtifactId);
			IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(destinationWorkspaceArtifactId);
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(destinationWorkspaceArtifactId);

			const string errorMessage = "Unable to create Relativity Source Job object. Please contact the system administrator.";
			int sourceJobDescriptorArtifactTypeId = CreateObjectType(destinationWorkspaceArtifactId, sourceJobRepository, artifactGuidRepository, sourceWorkspaceArtifactTypeId, errorMessage);
			var fieldGuids = new List<Guid>(2) { SourceJobDTO.Fields.JobHistoryIdFieldGuid, SourceJobDTO.Fields.JobHistoryNameFieldGuid };
			CreateObjectFields(fieldGuids, artifactGuidRepository, sourceJobRepository, fieldRepository, sourceJobDescriptorArtifactTypeId, errorMessage);
			CreateDocumentsFields(sourceJobDescriptorArtifactTypeId, SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid, artifactGuidRepository, sourceJobRepository, fieldRepository, errorMessage);

			// Create instance of Job History object
			SourceWorkspaceJobHistoryDTO sourceWorkspaceJobHistoryDto = sourceWorkspaceJobHistoryRepository.Retrieve(jobHistoryArtifactId);
			var jobHistoryDto = new SourceJobDTO()
			{
				Name = Utils.GetFormatForWorkspaceOrJobDisplay(sourceWorkspaceJobHistoryDto.Name, jobHistoryArtifactId),
				SourceWorkspaceArtifactId = sourceWorkspaceRdoInstanceArtifactId,
				JobHistoryArtifactId = jobHistoryArtifactId,
				JobHistoryName = sourceWorkspaceJobHistoryDto.Name,
			};

			int artifactId = sourceJobRepository.Create(sourceJobDescriptorArtifactTypeId, jobHistoryDto);
			jobHistoryDto.ArtifactId = artifactId;

			return jobHistoryDto;
		}

		protected override IDictionary<Guid, FieldDefinition> GetObjectFieldDefinitions()
		{
			return new Dictionary<Guid, FieldDefinition>()
			{
				{
					SourceJobDTO.Fields.JobHistoryIdFieldGuid, new FieldDefinition()
					{
						FieldName = IntegrationPoints.Contracts.Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME,
						FieldType = FieldType.WholeNumber
					}
				},
				{
					SourceJobDTO.Fields.JobHistoryNameFieldGuid, new FieldDefinition()
					{
						FieldName = IntegrationPoints.Contracts.Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME,
						FieldType = FieldType.FixedLengthText
					}
				}
			};
		}
	}
}