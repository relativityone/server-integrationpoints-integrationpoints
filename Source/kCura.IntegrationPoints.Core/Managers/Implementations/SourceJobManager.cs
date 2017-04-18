﻿using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceJobManager : DestinationWorkspaceFieldManagerBase, ISourceJobManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public SourceJobManager(IRepositoryFactory repositoryFactory)
			: base(repositoryFactory,
				  IntegrationPoints.Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME,
				  SourceJobDTO.ObjectTypeGuid,
				  "Unable to create Relativity Source Job object. Please contact the system administrator.")
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
			IRdoRepository rdoRepository = _repositoryFactory.GetRdoRepository(sourceWorkspaceArtifactId);
			IArtifactGuidRepository artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(destinationWorkspaceArtifactId);
			IFieldQueryRepository fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(destinationWorkspaceArtifactId);
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(destinationWorkspaceArtifactId);

			int sourceJobDescriptorArtifactTypeId = CreateObjectType(destinationWorkspaceArtifactId, sourceJobRepository, artifactGuidRepository, sourceWorkspaceArtifactTypeId);
			var fieldGuids = new List<Guid>(2) { SourceJobDTO.Fields.JobHistoryIdFieldGuid, SourceJobDTO.Fields.JobHistoryNameFieldGuid };
			CreateObjectFields(fieldGuids, artifactGuidRepository, sourceJobRepository, fieldQueryRepository, sourceJobDescriptorArtifactTypeId);
			CreateDocumentsFields(sourceJobDescriptorArtifactTypeId, SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid, artifactGuidRepository, sourceJobRepository, fieldQueryRepository, fieldRepository);

			// Create instance of Job History object
			RDO jobHistoryRdo = rdoRepository.ReadSingle(jobHistoryArtifactId);
			var jobHistoryDto = new SourceJobDTO()
			{
				ArtifactTypeId = sourceJobDescriptorArtifactTypeId,
				Name = Utils.GetFormatForWorkspaceOrJobDisplay(jobHistoryRdo.TextIdentifier, jobHistoryArtifactId),
				SourceWorkspaceArtifactId = sourceWorkspaceRdoInstanceArtifactId,
				JobHistoryArtifactId = jobHistoryArtifactId,
				JobHistoryName = jobHistoryRdo.TextIdentifier,
			};

			int artifactId = sourceJobRepository.Create(jobHistoryDto);
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
						FieldName = IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYID_FIELD_NAME,
						FieldType = FieldTypes.WholeNumber
					}
				},
				{
					SourceJobDTO.Fields.JobHistoryNameFieldGuid, new FieldDefinition()
					{
						FieldName = IntegrationPoints.Domain.Constants.SOURCEJOB_JOBHISTORYNAME_FIELD_NAME,
						FieldType = FieldTypes.FixedLengthText
					}
				}
			};
		}
	}
}