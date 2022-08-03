using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
    public class ArtifactServiceFactory : IArtifactServiceFactory
    {
        private readonly IRelativityObjectManagerFactory _objectManagerFactory;

        public ArtifactServiceFactory(IRelativityObjectManagerFactory objectManagerFactory)
        {
            _objectManagerFactory = objectManagerFactory;
        }

        public IArtifactService CreateArtifactService(IHelper helper)
        {
            return new ArtifactService(_objectManagerFactory, helper);
        }
    }
}
