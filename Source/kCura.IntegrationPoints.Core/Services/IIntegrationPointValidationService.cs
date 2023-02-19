using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    /// <summary>
    /// Validates Integration Point's model
    /// </summary>
    public interface IIntegrationPointValidationService
    {
        /// <summary>
        /// Performs quick validation of the Integration Point model
        /// </summary>
        /// <param name="model">Integration Point model to validate</param>
        /// <returns>Validation result</returns>
        ValidationResult Prevalidate(IntegrationPointProviderValidationModel model);

        /// <summary>
        /// Performs full validation of the Integration Point model
        /// </summary>
        /// <param name="model">Integration Point model to validate</param>
        /// <returns>Validation result</returns>
        /// <remarks>This validation will be performed while saving Integration Point</remarks>
        ValidationResult Validate(IntegrationPointProviderValidationModel model);
    }
}
