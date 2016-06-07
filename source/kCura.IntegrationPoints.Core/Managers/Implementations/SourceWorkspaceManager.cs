using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class SourceWorkspaceManager : DestinationWorkspaceFieldManagerBase, ISourceWorkspaceManager
	{
		public SourceWorkspaceManager(IRepositoryFactory repositoryFactory)
			: base (repositoryFactory, IntegrationPoints.Contracts.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, SourceWorkspaceDTO.ObjectTypeGuid)
		{
		}

		public SourceWorkspaceDTO InitializeWorkspace(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId)
		{
			ISourceWorkspaceRepository sourceWorkspaceRepository = RepositoryFactory.GetSourceWorkspaceRepository(destinationWorkspaceArtifactId);
			IArtifactGuidRepository artifactGuidRepository = RepositoryFactory.GetArtifactGuidRepository(destinationWorkspaceArtifactId);

			int sourceWorkspaceDescriptorArtifactTypeId = CreateObjectType(destinationWorkspaceArtifactId, sourceWorkspaceRepository, artifactGuidRepository, (int)ArtifactType.Case, Constants.RelativityProvider.ERROR_CREATE_SOURCE_CASE_FIELDS_ON_DESTINATION_CASE);

			IFieldRepository fieldRepository = RepositoryFactory.GetFieldRepository(destinationWorkspaceArtifactId);
			var fieldGuids = new List<Guid>(2) { SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid, SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid };
			CreateObjectFields(fieldGuids, artifactGuidRepository, sourceWorkspaceRepository, fieldRepository, sourceWorkspaceDescriptorArtifactTypeId, Constants.RelativityProvider.ERROR_CREATE_SOURCE_CASE_FIELDS_ON_DESTINATION_CASE);
			CreateDocumentsFields(sourceWorkspaceDescriptorArtifactTypeId, SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid, artifactGuidRepository, sourceWorkspaceRepository, fieldRepository, Constants.RelativityProvider.ERROR_CREATE_SOURCE_CASE_FIELDS_ON_DESTINATION_CASE);

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
			return new Dictionary<Guid, FieldDefinition>()
			{
				{
					SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid,
					new FieldDefinition()
					{
						FieldName = IntegrationPoints.Contracts.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME,
						FieldType = Relativity.Client.FieldType.WholeNumber
					}
				},
				{
					SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid,
					new FieldDefinition()
					{
						FieldName = IntegrationPoints.Contracts.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME,
						FieldType = Relativity.Client.FieldType.FixedLengthText
					}
				}

			};
		}
	}
}