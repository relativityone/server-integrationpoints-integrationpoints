using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public interface IRemoveSecuredConfigurationFromIntegrationPointService
    {
        bool RemoveSecuredConfiguration(IntegrationPoint integrationPoint);
    }
}