using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Checkers
{
    public interface ICustomProviderFlowCheck
    {
        bool ShouldBeUsed(DestinationConfiguration destinationConfiguration);
    }
}
