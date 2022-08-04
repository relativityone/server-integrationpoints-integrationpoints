using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class ArtifactGuidManager : IArtifactGuidManager
    {
        private readonly IRepositoryFactory _repositoryFactory;

        internal ArtifactGuidManager(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }
        
        public Dictionary<int, Guid> GetGuidsForArtifactIds(int workspaceArtifactId, IEnumerable<int> artifactIds)
        {
            IArtifactGuidRepository repository = _repositoryFactory.GetArtifactGuidRepository(workspaceArtifactId);
            return repository.GetGuidsForArtifactIds(artifactIds);
        }

        public Dictionary<Guid, int> GetArtifactIdsForGuids(int workspaceArtifactId, IEnumerable<Guid> guids)
        {
            IArtifactGuidRepository repository = _repositoryFactory.GetArtifactGuidRepository(workspaceArtifactId);
            return repository.GetArtifactIdsForGuids(guids);
        }
    }
}
