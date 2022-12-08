using kCura.IntegrationPoints.Web.Models.Validation;

namespace kCura.IntegrationPoints.Web.Models
{
    public class ValidatedProfileDTO
    {
        public IntegrationPointProfileWebModel Model { get; }

        public ValidationResultDTO ValidationResult { get; }

        public ValidatedProfileDTO(IntegrationPointProfileWebModel profileWebModel, ValidationResultDTO validationResult)
        {
            Model = profileWebModel;
            ValidationResult = validationResult;
        }
    }
}
