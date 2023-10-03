using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Checkers
{
    public interface ICustomProviderFlowCheck
    {
        bool ShouldBeUsed(IntegrationPointDto integrationPoint);
    }
}
