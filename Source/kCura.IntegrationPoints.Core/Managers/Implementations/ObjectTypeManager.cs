using System;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class ObjectTypeManager : IObjectTypeManager
    {
        private readonly IRepositoryFactory _repositoryFactory;

        internal ObjectTypeManager(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public int RetrieveObjectTypeDescriptorArtifactTypeId(int workspaceArtifactId, Guid objectTypeGuid)
        {
            IObjectTypeRepository repository = _repositoryFactory.GetObjectTypeRepository(workspaceArtifactId);
            return repository.RetrieveObjectTypeDescriptorArtifactTypeId(objectTypeGuid);
        }
    }
}
