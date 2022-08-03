using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Helpers
{
    public interface INonValidCharactersValidator
    {
        ValidationResult Validate(string name, string errorMessage);
    }
}
