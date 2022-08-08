using System;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class SourceJobRepository : ISourceJobRepository
    {
        private readonly IObjectTypeRepository _objectTypeRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly IRelativityObjectManager _objectManager;

        public SourceJobRepository(IObjectTypeRepository objectTypeRepository, IFieldRepository fieldRepository, IRelativityObjectManager objectManager)
        {
            _objectTypeRepository = objectTypeRepository;
            _fieldRepository = fieldRepository;
            _objectManager = objectManager;
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
                return _objectManager.Create(sourceJobDto.ObjectTypeRef, sourceJobDto.ParentObject, sourceJobDto.FieldRefValuePairs);
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