using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Checkers
{
    internal interface ICustomProviderFlowCheck
    {
        bool ShouldBeUsed(IntegrationPointDto integrationPoint);
    }
}
