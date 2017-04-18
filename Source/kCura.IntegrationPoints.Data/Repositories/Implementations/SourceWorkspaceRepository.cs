using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SourceWorkspaceRepository : ISourceWorkspaceRepository
	{
		private readonly IObjectTypeRepository _objectTypeRepository;
		private readonly IFieldRepository _fieldRepository;
		private readonly IRdoRepository _rdoRepository;

		public SourceWorkspaceRepository(IObjectTypeRepository objectTypeRepository, IFieldRepository fieldRepository, IRdoRepository rdoRepository)
		{
			_objectTypeRepository = objectTypeRepository;
			_fieldRepository = fieldRepository;
			_rdoRepository = rdoRepository;
		}

		public int CreateObjectType(int parentArtifactTypeId)
		{
			try
			{
				return _objectTypeRepository.CreateObjectType(SourceWorkspaceDTO.ObjectTypeGuid, Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, parentArtifactTypeId);
			}
			catch (Exception e)
			{
				throw new Exception(RelativityProvider.ERROR_CREATE_SOURCE_CASE_FIELDS_ON_DESTINATION_CASE, e);
			}
		}

		public IDictionary<Guid, int> CreateObjectTypeFields(int sourceWorkspaceObjectTypeId, IEnumerable<Guid> fieldGuids)
		{
			try
			{
				var objectType = new ObjectType {DescriptorArtifactTypeID = sourceWorkspaceObjectTypeId};

				var sourceWorkspaceFields = SourceWorkspaceDTO.Fields.GetFieldsDefinition(objectType);

				var fieldsToCreate = sourceWorkspaceFields.Where(x => fieldGuids.Contains(x.Key)).Select(x => x.Value).ToList();

				var newFields = _fieldRepository.CreateObjectTypeFields(fieldsToCreate);

				IDictionary<Guid, int> guidToIdDictionary = newFields.ToDictionary(
					x => x.Guids[0],
					y => y.ArtifactID);

				return guidToIdDictionary;
			}
			catch (Exception e)
			{
				throw new Exception(RelativityProvider.ERROR_CREATE_SOURCE_CASE_FIELDS_ON_DESTINATION_CASE, e);
			}
		}

		public int CreateFieldOnDocument(int sourceWorkspaceObjectTypeId)
		{
			try
			{
				return _fieldRepository.CreateMultiObjectFieldOnDocument(Domain.Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME, sourceWorkspaceObjectTypeId);
			}
			catch (Exception e)
			{
				throw new Exception(RelativityProvider.ERROR_CREATE_SOURCE_CASE_FIELDS_ON_DESTINATION_CASE, e);
			}
		}

		public SourceWorkspaceDTO RetrieveForSourceWorkspaceId(int sourceWorkspaceArtifactId, string federatedInstanceName, int? federatedInstanceArtifactId)
		{
			var sourceWorkspaceCondition = new WholeNumberCondition(Domain.Constants.SOURCEWORKSPACE_CASEID_FIELD_NAME, NumericConditionEnum.EqualTo, sourceWorkspaceArtifactId);
			Condition federatedInstanceCondition;
			if (federatedInstanceArtifactId.HasValue)
			{
				federatedInstanceCondition = new TextCondition(Domain.Constants.SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME, TextConditionEnum.EqualTo, federatedInstanceName);
			}
			else
			{
				var instanceNameCondition = new TextCondition(Domain.Constants.SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME, TextConditionEnum.EqualTo, federatedInstanceName);
				var instanceNameIsNotSetCondition = new NotCondition(new TextCondition(Domain.Constants.SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME, TextConditionEnum.IsSet));
				federatedInstanceCondition = new CompositeCondition(instanceNameCondition, CompositeConditionEnum.Or, instanceNameIsNotSetCondition);
			}


			var query = new Query<RDO>
			{
				ArtifactTypeGuid = SourceWorkspaceDTO.ObjectTypeGuid,
				Fields = FieldValue.AllFields,
				Condition = new CompositeCondition(sourceWorkspaceCondition, CompositeConditionEnum.And, federatedInstanceCondition)
			};

			RDO rdo;
			try
			{
				rdo = _rdoRepository.QuerySingle(query);
			}
			catch
			{
				return null;
			}

			return new SourceWorkspaceDTO(rdo.ArtifactID, rdo.Fields);
		}

		public int Create(SourceWorkspaceDTO sourceWorkspaceDto)
		{
			try
			{
				return _rdoRepository.Create(sourceWorkspaceDto.ToRdo());
			}
			catch (Exception e)
			{
				throw new Exception("Unable to create new instance of Source Workspace", e);
			}
		}

		public void Update(SourceWorkspaceDTO sourceWorkspaceDto)
		{
			try
			{
				_rdoRepository.Update(sourceWorkspaceDto.ToRdo());
			}
			catch (Exception e)
			{
				throw new Exception("Unable to update Source Workspace instance", e);
			}
		}
	}
}