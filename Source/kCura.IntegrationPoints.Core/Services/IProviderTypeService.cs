using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IProviderTypeService
    {
        ProviderType GetProviderType(int sourceProviderId, int destinationProviderId);

        ProviderType GetProviderType(Data.IntegrationPoint integrationPoint);

        string GetProviderName(int sourceProviderId, int destinationProviderId);

    }
}