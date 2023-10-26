using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Checkers
{
    public interface ICustomProviderFlowCheck
    {
        bool ShouldBeUsed(int integrationPointId, ProviderType providerType);

        bool ShouldBeUsed(DestinationConfiguration destinationConfiguration, ProviderType? providerType = null);
    }
}
