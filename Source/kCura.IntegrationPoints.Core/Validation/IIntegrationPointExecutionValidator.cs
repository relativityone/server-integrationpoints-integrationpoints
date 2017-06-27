using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation
{
    public interface IIntegrationPointExecutionValidator
    {
        ValidationResult Validate(IntegrationPointModel integrationModel);
    }
}