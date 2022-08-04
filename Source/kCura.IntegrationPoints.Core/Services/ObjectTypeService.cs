using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public class ObjectTypeService
    {
        private readonly IObjectTypeRepository _objectTypeRepository;
        public ObjectTypeService(IObjectTypeRepository objectTypeRepository)
        {
            _objectTypeRepository = objectTypeRepository;
        }

        public bool HasParent(int objectType)
        {
            ObjectTypeDTO rdo = _objectTypeRepository.GetObjectType(objectType);
            return rdo.ParentArtifactTypeId > Data.Constants.NON_SYSTEM_FIELD_START_ID;
        }

        public int GetObjectTypeID(string artifactTypeName)
        {
            return _objectTypeRepository.GetObjectTypeID(artifactTypeName);
        }

    }
}
