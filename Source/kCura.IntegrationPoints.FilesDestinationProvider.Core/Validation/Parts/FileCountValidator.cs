using System;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class FileCountValidator : BaseValidator<int>
	{
		public override ValidationResult Validate(int value)
		{
			return (value > 0) ? new ValidationResult() : new ValidationResult(true, ValidationMessages.FILE_COUNT_WARNING);
		}
	}
}