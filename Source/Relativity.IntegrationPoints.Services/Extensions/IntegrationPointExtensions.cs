using kCura.IntegrationPoints.Core.Models;

namespace Relativity.IntegrationPoints.Services.Extensions
{
    public static class IntegrationPointExtensions
    {
        public static IntegrationPointModel ToIntegrationPointModel(this IntegrationPointDto data)
        {
            return new IntegrationPointModel()
            {
                ArtifactId = data.ArtifactId,
                Name = data.Name,
                SourceProvider = data.SourceProvider,
                DestinationProvider = data.DestinationProvider,
                SecuredConfiguration = data.SecuredConfiguration,
            };
        }

        public static IntegrationPointModel ToIntegrationPointModel(this IntegrationPointProfileDto data)
        {
            return new IntegrationPointModel()
            {
                ArtifactId = data.ArtifactId,
                Name = data.Name,
                SourceProvider = data.SourceProvider,
                DestinationProvider = data.DestinationProvider,
            };
        }
    }
}
