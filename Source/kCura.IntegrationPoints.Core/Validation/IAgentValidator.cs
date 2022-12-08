using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Validation
{
    public interface IAgentValidator
    {
        void Validate(IntegrationPointDto integrationPointDto, int submittedByUserId);
    }
}
