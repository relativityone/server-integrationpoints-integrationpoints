using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceWorkspaceManager : DestinationWorkspaceFieldManagerBase, ISourceWorkspaceManager
	{
		public SourceWorkspaceManager(IRepositoryFactory repositoryFactory)
			: base(repositoryFactory,
				  IntegrationPoints.Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
				  SourceWorkspaceDTO.ObjectTypeGuid,
				  Constants.RelativityProvider.ERROR_CREATE_SOURCE_CASE_FIELDS_ON_DESTINATION_CASE)
		{
		}

		public SourceWorkspaceDTO InitializeWorkspace(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId)
		{
			ISourceWorkspaceRepository sourceWorkspaceRepository = RepositoryFactory.GetSourceWorkspaceRepository(destinationWorkspaceArtifactId);
			IArtifactGuidRepository artifactGuidRepository = RepositoryFactory.GetArtifactGuidRepository(destinationWorkspaceArtifactId);

			int sourceWorkspaceDescriptorArtifactTypeId = CreateObjectType(destinationWorkspaceArtifactId, sourceWorkspaceRepository, artifactGuidRepository, (int)ArtifactType.Case);

			IExtendedFieldRepository fieldRepository = RepositoryFactory.GetExtendedFieldRepository(destinationWorkspaceArtifactId);
			var fieldGuids = new List<Guid>(2) { SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid, SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid };
			CreateObjectFields(fieldGuids, artifactGuidRepository, sourceWorkspaceRepository, fieldRepository, sourceWorkspaceDescriptorArtifactTypeId);
			CreateDocumentsFields(sourceWorkspaceDescriptorArtifactTypeId, SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid, artifactGuidRepository, sourceWorkspaceRepository, fieldRepository);

			SourceWorkspaceDTO sourceWorkspaceDto = CreateSourceWorkspaceDto(sourceWorkspaceArtifactId, sourceWorkspaceDescriptorArtifactTypeId, sourceWorkspaceRepository);
			return sourceWorkspaceDto;
		}

		private SourceWorkspaceDTO CreateSourceWorkspaceDto(int workspaceArtifactId,
			int sourceWorkspaceDescriptorArtifactTypeId, ISourceWorkspaceRepository sourceWorkspaceRepository)
		{
			IWorkspaceRepository workspaceRepository = RepositoryFactory.GetWorkspaceRepository();
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

		protected override IDictionary<Guid, FieldDefinition> GetObjectFieldDefinitions()
		{
			return new Dictionary<Guid, FieldDefinition>
			{
				{
					SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid,
					new FieldDefinition
					{
						FieldName = IntegrationPoints.Domain.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME,
						FieldType = Relativity.Client.FieldType.WholeNumber
					}
				},
				{
					SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid,
					new FieldDefinition
					{
						FieldName = IntegrationPoints.Domain.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME,
						FieldType = Relativity.Client.FieldType.FixedLengthText
					}
				}
			};
		}
	}
}