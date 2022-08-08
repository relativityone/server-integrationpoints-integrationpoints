using kCura.IntegrationPoints.Core.Services.IntegrationPoint;

namespace Relativity.IntegrationPoints.Services.Helpers
{
    public interface IIntegrationPointRuntimeServiceFactory
    {
        IIntegrationPointService CreateIntegrationPointRuntimeService(kCura.IntegrationPoints.Core.Models.IntegrationPointModel model);
    }
}
