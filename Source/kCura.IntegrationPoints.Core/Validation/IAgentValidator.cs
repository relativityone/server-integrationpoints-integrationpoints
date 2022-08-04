using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Validation
{
    public interface IAgentValidator
    {
        void Validate(IntegrationPoint integrationPointDto, int submittedByUserId);
    }
}
