using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IProviderTypeService
    {
        ProviderType GetProviderType(int sourceProviderId, int destinationProviderId);
        
        string GetProviderName(int sourceProviderId, int destinationProviderId);
    }
}