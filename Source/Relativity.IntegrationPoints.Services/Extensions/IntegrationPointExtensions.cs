using kCura.IntegrationPoints.Data;

namespace Relativity.IntegrationPoints.Services.Extensions
{
    public static class IntegrationPointExtensions
    {
        public static IntegrationPointModel ToIntegrationPointModel(this IntegrationPoint data)
        {
            return new IntegrationPointModel()
            {
                ArtifactId = data.ArtifactId,
                Name = data.Name,
                SourceProvider = data.SourceProvider ?? 0,
                DestinationProvider = data.DestinationProvider ?? 0,
                SecuredConfiguration = data.SecuredConfiguration
            };
        }

        public static IntegrationPointModel ToIntegrationPointModel(this IntegrationPointProfile data)
        {
            return new IntegrationPointModel()
            {
                ArtifactId = data.ArtifactId,
                Name = data.Name,
                SourceProvider = data.SourceProvider ?? 0,
                DestinationProvider = data.DestinationProvider ?? 0
            };
        }
    }
}