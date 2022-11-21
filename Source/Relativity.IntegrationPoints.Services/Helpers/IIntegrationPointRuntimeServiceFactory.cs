using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;

namespace Relativity.IntegrationPoints.Services.Helpers
{
    public interface IIntegrationPointRuntimeServiceFactory
    {
        IIntegrationPointService CreateIntegrationPointRuntimeService(IntegrationPointDto dto);
    }
}
