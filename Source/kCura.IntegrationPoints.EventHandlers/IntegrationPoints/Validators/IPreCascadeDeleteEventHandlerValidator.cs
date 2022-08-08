
namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators
{
    public interface IPreCascadeDeleteEventHandlerValidator
    {
        void Validate(int workspaceId, int integrationPointId);
    }
}
