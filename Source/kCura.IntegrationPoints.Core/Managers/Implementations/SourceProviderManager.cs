using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class SourceProviderManager : ISourceProviderManager
    {
        private readonly IRepositoryFactory _repositoryFactory;

        internal SourceProviderManager(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public int GetArtifactIdFromSourceProviderTypeGuidIdentifier(int workspaceArtifactId, string sourceProviderGuidIdentifier)
        {
            ISourceProviderRepository sourceProviderRepository = _repositoryFactory.GetSourceProviderRepository(workspaceArtifactId);
            return sourceProviderRepository.GetArtifactIdFromSourceProviderTypeGuidIdentifier(sourceProviderGuidIdentifier);
        }
    }
}