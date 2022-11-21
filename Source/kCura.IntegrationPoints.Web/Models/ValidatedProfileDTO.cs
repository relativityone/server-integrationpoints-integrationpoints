using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Web.Models.Validation;

namespace kCura.IntegrationPoints.Web.Models
{
    public class ValidatedProfileDTO
    {
        public IntegrationPointProfileDto Dto { get; }

        public ValidationResultDTO ValidationResult { get; }

        public ValidatedProfileDTO(IntegrationPointProfileDto dto, ValidationResultDTO validationResult)
        {
            Dto = dto;
            ValidationResult = validationResult;
        }
    }
}
