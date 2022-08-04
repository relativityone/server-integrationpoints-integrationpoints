using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
    public class FileCountValidator : BasePartsValidator<long>
    {
        public override ValidationResult Validate(long value)
        {
            return (value > 0) ? new ValidationResult() : new ValidationResult(false, FileDestinationProviderValidationMessages.FILE_COUNT_WARNING);
        }
    }
}