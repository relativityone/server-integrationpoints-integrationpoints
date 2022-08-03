using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public static class IntegrationPointExtensions
    {
        public static ProviderType GetProviderType(this Data.IntegrationPoint integrationPoint, IProviderTypeService providerTypeService)
        {
            ProviderType providerType = providerTypeService.GetProviderType(
                integrationPoint.SourceProvider.GetValueOrDefault(),
                integrationPoint.DestinationProvider.GetValueOrDefault());
            return providerType;
        }

        public static string GetProviderName(this Data.IntegrationPoint integrationPoint, IProviderTypeService providerTypeService) =>
            providerTypeService.GetProviderName(
                integrationPoint.SourceProvider.GetValueOrDefault(),
                integrationPoint.DestinationProvider.GetValueOrDefault());
    }
}