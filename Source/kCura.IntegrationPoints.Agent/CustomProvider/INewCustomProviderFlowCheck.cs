using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    internal interface INewCustomProviderFlowCheck
    {
        Task<bool> ShouldBeUsedAsync(IntegrationPointDto integrationPoint);
    }
}
