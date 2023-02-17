using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
    public class RelativitySourceRdoObjectType : IRelativitySourceRdoObjectType
    {
        private readonly IRelativityProviderObjectRepository _relativityObjectRepository;
        private readonly IRepositoryFactory _repositoryFactory;
        private IObjectTypeRepository _objectTypeRepository;
        private IArtifactGuidRepository _artifactGuidRepository;
        private ITabRepository _tabRepository;

        public RelativitySourceRdoObjectType(IRelativityProviderObjectRepository relativityObjectRepository, IRepositoryFactory repositoryFactory)
        {
            _relativityObjectRepository = relativityObjectRepository;
            _repositoryFactory = repositoryFactory;
        }

        public int CreateObjectType(int workspaceArtifactId, Guid objectTypeGuid, string objectTypeName, int parentArtifactTypeId)
        {
            _objectTypeRepository = _repositoryFactory.GetDestinationObjectTypeRepository(workspaceArtifactId);
            _artifactGuidRepository = _repositoryFactory.GetArtifactGuidRepository(workspaceArtifactId);
            _tabRepository = _repositoryFactory.GetTabRepository(workspaceArtifactId);

            if (ObjectTypeWithGuidExists(objectTypeGuid))
            {
                return GetExistingObjectTypeDescriptorArtifactId(objectTypeGuid);
            }

            int objectTypeArtifactId;
            if (ObjectTypeWithoutGuidExists(objectTypeName))
            {
                objectTypeArtifactId = GetExistingObjectTypeArtifactId(objectTypeName);
            }
            else
            {
                objectTypeArtifactId = CreateObjectTypeWithoutGuid(parentArtifactTypeId);
            }

            AssignGuidToObjectType(objectTypeArtifactId, objectTypeGuid);

            int descriptorArtifactTypeId = GetExistingObjectTypeDescriptorArtifactId(objectTypeGuid);

            DeleteObjectTypeTab(descriptorArtifactTypeId, objectTypeName);

            return descriptorArtifactTypeId;
        }

        private bool ObjectTypeWithGuidExists(Guid objectTypeGuid)
        {
            try
            {
                _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(objectTypeGuid);
                return true;
            }
            catch (TypeLoadException)
            {
                return false;
            }
        }

        private int GetExistingObjectTypeDescriptorArtifactId(Guid objectTypeGuid)
        {
            return _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(objectTypeGuid);
        }

        private int GetExistingObjectTypeArtifactId(string objectTypeName)
        {
            return _objectTypeRepository.RetrieveObjectTypeArtifactId(objectTypeName).Value;
        }

        private bool ObjectTypeWithoutGuidExists(string objectTypeName)
        {
            int? objectTypeArtifactId = _objectTypeRepository.RetrieveObjectTypeArtifactId(objectTypeName);

            return objectTypeArtifactId.HasValue;
        }

        private int CreateObjectTypeWithoutGuid(int parentArtifactTypeId)
        {
            return _relativityObjectRepository.CreateObjectType(parentArtifactTypeId);
        }

        private void AssignGuidToObjectType(int objectTypeArtifactId, Guid objectTypeGuid)
        {
            _artifactGuidRepository.InsertArtifactGuidForArtifactId(objectTypeArtifactId, objectTypeGuid);
        }

        private void DeleteObjectTypeTab(int objectDescriptorArtifactTypeId, string objectTypeName)
        {
            int? tabId = _tabRepository.RetrieveTabArtifactId(objectDescriptorArtifactTypeId, objectTypeName);
            if (tabId.HasValue)
            {
                _tabRepository.Delete(tabId.Value);
            }
        }
    }
}
