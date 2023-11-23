using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Core.Factories
{
    public interface IArtifactServiceFactory
    {
        IArtifactService CreateArtifactService();
    }
}
