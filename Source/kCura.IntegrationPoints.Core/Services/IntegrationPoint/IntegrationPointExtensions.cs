using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public static class IntegrationPointProviderTypeExtensions
    {
        public static ProviderType GetProviderType(this IntegrationPointSlimDto integrationPoint, IProviderTypeService providerTypeService)
        {
            ProviderType providerType = providerTypeService.GetProviderType(
                integrationPoint.SourceProvider,
                integrationPoint.DestinationProvider);
            return providerType;
        }

        public static string GetProviderName(this IntegrationPointSlimDto integrationPoint, IProviderTypeService providerTypeService) =>
            providerTypeService.GetProviderName(
                integrationPoint.SourceProvider,
                integrationPoint.DestinationProvider);

        public static ProviderType GetProviderType(this IntegrationPointDto integrationPoint, IProviderTypeService providerTypeService)
        {
            ProviderType providerType = providerTypeService.GetProviderType(
                integrationPoint.SourceProvider,
                integrationPoint.DestinationProvider);
            return providerType;
        }

        public static string GetProviderName(this IntegrationPointDto integrationPoint, IProviderTypeService providerTypeService) =>
            providerTypeService.GetProviderName(
                integrationPoint.SourceProvider,
                integrationPoint.DestinationProvider);
    }
}
