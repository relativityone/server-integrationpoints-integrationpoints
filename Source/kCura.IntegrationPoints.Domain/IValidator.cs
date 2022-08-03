using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Domain
{
    public interface IValidator
    {
        string Key { get; }

        ValidationResult Validate(object value);
    }
}