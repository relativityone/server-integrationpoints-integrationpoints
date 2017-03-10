using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Data;
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

		public SourceWorkspaceDTO InitializeWorkspace(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, int? federatedInstanceArtifactId)
		{
			ISourceWorkspaceRepository sourceWorkspaceRepository = RepositoryFactory.GetSourceWorkspaceRepository(destinationWorkspaceArtifactId);
			IArtifactGuidRepository artifactGuidRepository = RepositoryFactory.GetArtifactGuidRepository(destinationWorkspaceArtifactId);
			IFieldRepository fieldRepository = RepositoryFactory.GetFieldRepository(destinationWorkspaceArtifactId);

			int sourceWorkspaceDescriptorArtifactTypeId = CreateObjectType(destinationWorkspaceArtifactId, sourceWorkspaceRepository, artifactGuidRepository, (int) ArtifactType.Case);
			
			var fieldGuids = new List<Guid>(3)
			{
				SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid,
				SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid,
				SourceWorkspaceDTO.Fields.InstanceNameFieldGuid
			};
			CreateObjectFields(fieldGuids, artifactGuidRepository, sourceWorkspaceRepository, fieldRepository, sourceWorkspaceDescriptorArtifactTypeId);
			CreateDocumentsFields(sourceWorkspaceDescriptorArtifactTypeId, SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid, artifactGuidRepository, sourceWorkspaceRepository,
				fieldRepository);

			SourceWorkspaceDTO sourceWorkspaceDto = CreateSourceWorkspaceDto(sourceWorkspaceArtifactId, sourceWorkspaceDescriptorArtifactTypeId, federatedInstanceArtifactId,
				sourceWorkspaceRepository);
			return sourceWorkspaceDto;
		}

		private SourceWorkspaceDTO CreateSourceWorkspaceDto(int workspaceArtifactId,
			int sourceWorkspaceDescriptorArtifactTypeId, int? federatedInstanceArtifactId, ISourceWorkspaceRepository sourceWorkspaceRepository)
		{
			IWorkspaceRepository workspaceRepository = RepositoryFactory.GetSourceWorkspaceRepository();
			WorkspaceDTO workspaceDto = workspaceRepository.Retrieve(workspaceArtifactId);

			string currentInstanceName = FederatedInstanceManager.LocalInstance.Name;
			if (federatedInstanceArtifactId.HasValue)
			{
				IInstanceSettingRepository instanceSettingRepository = RepositoryFactory.GetInstanceSettingRepository();
				currentInstanceName = instanceSettingRepository.GetConfigurationValue("Relativity.Authentication", "FriendlyInstanceName");
			}

			SourceWorkspaceDTO sourceWorkspaceDto = sourceWorkspaceRepository.RetrieveForSourceWorkspaceId(workspaceArtifactId, currentInstanceName, federatedInstanceArtifactId);
			
			if (sourceWorkspaceDto == null)
			{
				sourceWorkspaceDto = new SourceWorkspaceDTO
				{
					ArtifactId = -1,
					Name = Utils.GetFormatForWorkspaceOrJobDisplay(currentInstanceName, workspaceDto.Name, workspaceArtifactId),
					SourceCaseArtifactId = workspaceArtifactId,
					SourceCaseName = workspaceDto.Name,
					SourceInstanceName = currentInstanceName
				};

				int artifactId = sourceWorkspaceRepository.Create(sourceWorkspaceDescriptorArtifactTypeId, sourceWorkspaceDto);
				sourceWorkspaceDto.ArtifactId = artifactId;
			}

			sourceWorkspaceDto.ArtifactTypeId = sourceWorkspaceDescriptorArtifactTypeId;

			// Check to see if instance should be updated
			if (sourceWorkspaceDto.SourceCaseName != workspaceDto.Name || sourceWorkspaceDto.SourceInstanceName != currentInstanceName)
			{
				sourceWorkspaceDto.Name = Utils.GetFormatForWorkspaceOrJobDisplay(currentInstanceName, workspaceDto.Name, workspaceArtifactId);
				sourceWorkspaceDto.SourceCaseName = workspaceDto.Name;
				sourceWorkspaceDto.SourceInstanceName = currentInstanceName;
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
						FieldType = FieldTypes.WholeNumber
					}
				},
				{
					SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid,
					new FieldDefinition
					{
						FieldName = IntegrationPoints.Domain.Constants.SOURCEWORKSPACE_CASENAME_FIELD_NAME,
						FieldType = FieldTypes.FixedLengthText
					}
				},
				{
					SourceWorkspaceDTO.Fields.InstanceNameFieldGuid,
					new FieldDefinition
					{
						FieldName = IntegrationPoints.Domain.Constants.SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME,
						FieldType = FieldTypes.FixedLengthText
					}
				}
			};
		}
	}
}