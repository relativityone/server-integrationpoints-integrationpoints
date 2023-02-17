using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Web.Models.Validation
{
    public interface IValidationResultMapper
    {
        ValidationResultDTO Map(ValidationResult validationResult);
    }
}
