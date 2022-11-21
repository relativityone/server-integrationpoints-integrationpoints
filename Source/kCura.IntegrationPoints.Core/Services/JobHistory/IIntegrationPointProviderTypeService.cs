using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public interface IIntegrationPointProviderTypeService
    {
        ProviderType GetProviderType(int integrationPointArtifactId);

        ProviderType GetProviderType(IntegrationPointDto integrationPoint);
    }
}
