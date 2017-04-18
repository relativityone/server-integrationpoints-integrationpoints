using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SourceJobRepository : ISourceJobRepository
	{
		private readonly IObjectTypeRepository _objectTypeRepository;
		private readonly IFieldRepository _fieldRepository;
		private readonly IRdoRepository _rdoRepository;

		public SourceJobRepository(IObjectTypeRepository objectTypeRepository, IFieldRepository fieldRepository, IRdoRepository rdoRepository)
		{
			_objectTypeRepository = objectTypeRepository;
			_fieldRepository = fieldRepository;
			_rdoRepository = rdoRepository;
		}

		public int CreateObjectType(int parentArtifactTypeId)
		{
			try
			{
				return _objectTypeRepository.CreateObjectType(SourceJobDTO.ObjectTypeGuid, Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, parentArtifactTypeId);
			}
			catch (Exception e)
			{
				throw new Exception(RelativityProvider.ERROR_CREATE_SOURCE_CASE_FIELDS_ON_DESTINATION_CASE, e);
			}
		}

		public int Create(SourceJobDTO sourceJobDto)
		{
			try
			{
				return _rdoRepository.Create(sourceJobDto.ToRdo());
			}
			catch (Exception e)
			{
				throw new Exception(RelativityProvider.ERROR_CREATE_SOURCE_CASE_FIELDS_ON_DESTINATION_CASE, e);
			}
		}

		public IDictionary<Guid, int> CreateObjectTypeFields(int sourceJobArtifactTypeId, IEnumerable<Guid> fieldGuids)
		{
			try
			{
				var objectType = new ObjectType {DescriptorArtifactTypeID = sourceJobArtifactTypeId};

				var sourceJobFields = SourceJobDTO.Fields.GetFieldsDefinition(objectType);

				List<Field> fieldsToCreate = sourceJobFields.Where(x => fieldGuids.Contains(x.Key)).Select(x => x.Value).ToList();

				List<Field> newFields = _fieldRepository.CreateObjectTypeFields(fieldsToCreate);

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

		public int CreateFieldOnDocument(int sourceJobArtifactTypeId)
		{
			try
			{
				return _fieldRepository.CreateMultiObjectFieldOnDocument(Domain.Constants.SPECIAL_SOURCEJOB_FIELD_NAME, sourceJobArtifactTypeId);
			}
			catch (Exception e)
			{
				throw new Exception(RelativityProvider.ERROR_CREATE_SOURCE_CASE_FIELDS_ON_DESTINATION_CASE, e);
			}
		}
	}
}