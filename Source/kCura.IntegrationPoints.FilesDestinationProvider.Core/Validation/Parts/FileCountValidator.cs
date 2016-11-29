using System;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class FileCountValidator : BasePartsValidator<int>
	{
		public override ValidationResult Validate(int value)
		{
			return (value > 0) ? new ValidationResult() : new ValidationResult(true, FileDestinationProviderValidationMessages.FILE_COUNT_WARNING);
		}
	}
}