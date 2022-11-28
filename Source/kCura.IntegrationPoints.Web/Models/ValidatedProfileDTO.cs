using kCura.IntegrationPoints.Web.Models.Validation;

namespace kCura.IntegrationPoints.Web.Models
{
    public class ValidatedProfileDTO
    {
        public IntegrationPointProfileWebModel Dto { get; }

        public ValidationResultDTO ValidationResult { get; }

        public ValidatedProfileDTO(IntegrationPointProfileWebModel profileWebModel, ValidationResultDTO validationResult)
        {
            Dto = profileWebModel;
            ValidationResult = validationResult;
        }
    }
}
