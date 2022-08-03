using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Web.Models.Validation;

namespace kCura.IntegrationPoints.Web.Models
{
    public class ValidatedProfileDTO
    {
        public IntegrationPointProfileModel Model { get; }
        public ValidationResultDTO ValidationResult { get; }

        public ValidatedProfileDTO(IntegrationPointProfileModel model, ValidationResultDTO validationResult)
        {
            Model = model;
            ValidationResult = validationResult;
        }
    }
}