using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation
{
    public interface IValidationExecutor
    {
        void ValidateOnRun(ValidationContext validationContext);

        void ValidateOnSave(ValidationContext validationContext);

        void ValidateOnStop(ValidationContext validationContext);

        ValidationResult ValidateOnProfile(ValidationContext validationContext);
    }
}
