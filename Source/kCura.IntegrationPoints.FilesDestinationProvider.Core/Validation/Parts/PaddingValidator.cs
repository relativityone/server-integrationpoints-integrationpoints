using System;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters.Validator;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class PaddingValidator : BaseValidator<ExportFile>
	{
		public override ValidationResult Validate(ExportFile value)
		{
			return Validate(value, 0);
		}

		public ValidationResult Validate(ExportFile value, int totalDocCount)
		{
			//Logic extracted from SharedLibrary
			var currentVolumeNumber = value.VolumeInfo.VolumeStartNumber;
			var currentSubdirectoryNumber = value.VolumeInfo.SubdirectoryStartNumber;

			var subdirectoryNumberPaddingWidth = (int)Math.Floor(Math.Log10(currentSubdirectoryNumber + 1) + 1);
			var volumeNumberPaddingWidth = (int)Math.Floor(Math.Log10(currentVolumeNumber + 1) + 1);
			var totalFilesNumberPaddingWidth = (int)Math.Floor(Math.Log10(totalDocCount + currentVolumeNumber + 1) + 1);
			var volumeLabelPaddingWidth = Math.Max(totalFilesNumberPaddingWidth, volumeNumberPaddingWidth);
			var subdirectoryLabelPaddingWidth = Math.Max(totalFilesNumberPaddingWidth, subdirectoryNumberPaddingWidth);

			var warningValidator = new PaddingWarningValidator();
			var isValid = warningValidator.IsValid(value, volumeLabelPaddingWidth, subdirectoryLabelPaddingWidth);

			// this validation results only in a warning
			return new ValidationResult(true, warningValidator.ErrorMessages);
		}
	}
}