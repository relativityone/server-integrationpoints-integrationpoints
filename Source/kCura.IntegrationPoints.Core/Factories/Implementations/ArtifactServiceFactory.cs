using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
    public class ArtifactServiceFactory : IArtifactServiceFactory
    {
        private readonly IRelativityObjectManagerFactory _objectManagerFactory;
        private readonly ILogger<ArtifactServiceFactory> _logger;

        public ArtifactServiceFactory(IRelativityObjectManagerFactory objectManagerFactory, ILogger<ArtifactServiceFactory> logger)
        {
            _objectManagerFactory = objectManagerFactory;
            _logger = logger;
        }

        public IArtifactService CreateArtifactService()
        {
            return new ArtifactService(_objectManagerFactory, _logger.ForContext<ArtifactService>());
        }
    }
}
