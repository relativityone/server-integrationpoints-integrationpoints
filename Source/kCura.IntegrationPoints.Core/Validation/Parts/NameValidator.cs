using kCura.IntegrationPoints.Core.Validation.Helpers;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Parts
{
    public class NameValidator : IValidator
    {
        private readonly INonValidCharactersValidator _nonValidCharactersValidator;

        public string Key => Constants.IntegrationPointProfiles.Validation.NAME;

        public NameValidator(INonValidCharactersValidator nonValidCharactersValidator)
        {
            _nonValidCharactersValidator = nonValidCharactersValidator;
        }

        public ValidationResult Validate(object value)
        {
            var result = new ValidationResult();

            var name = value as string;
            if (string.IsNullOrWhiteSpace(name))
            {
                result.Add(IntegrationPointProviderValidationMessages.ERROR_INTEGRATION_POINT_NAME_EMPTY);
            }
            else
            {
                string errorMessage = IntegrationPointProviderValidationMessages.ERROR_INTEGRATION_POINT_NAME_CONTAINS_ILLEGAL_CHARACTERS;
                ValidationResult isValidNameForDirectory = _nonValidCharactersValidator.Validate(name, errorMessage);
                result.Add(isValidNameForDirectory);
            }

            return result;
        }
    }
}
