using kCura.IntegrationPoints.Core.Services;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories
{
    public interface IArtifactServiceFactory
    {
        IArtifactService CreateArtifactService(IHelper helper);
    }
}
