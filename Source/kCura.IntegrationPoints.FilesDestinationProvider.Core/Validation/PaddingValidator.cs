﻿using System;
using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters.Validator;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class PaddingValidator : IPaddingValidator
	{
		public ValidationResult Validate(int workspaceId, ExportFile exportFile, int totalDocCount)
		{
			//Logic extracted from SharedLibrary
			var currentVolumeNumber = exportFile.VolumeInfo.VolumeStartNumber;
			var currentSubdirectoryNumber = exportFile.VolumeInfo.SubdirectoryStartNumber;

			var subdirectoryNumberPaddingWidth = (int)Math.Floor(Math.Log10(currentSubdirectoryNumber + 1) + 1);
			var volumeNumberPaddingWidth = (int)Math.Floor(Math.Log10(currentVolumeNumber + 1) + 1);
			var totalFilesNumberPaddingWidth = (int)Math.Floor(Math.Log10(totalDocCount + currentVolumeNumber + 1) + 1);
			var volumeLabelPaddingWidth = Math.Max(totalFilesNumberPaddingWidth, volumeNumberPaddingWidth);
			var subdirectoryLabelPaddingWidth = Math.Max(totalFilesNumberPaddingWidth, subdirectoryNumberPaddingWidth);

			var warningValidator = new PaddingWarningValidator();
			var isValid = warningValidator.IsValid(exportFile, volumeLabelPaddingWidth, subdirectoryLabelPaddingWidth);

			return new ValidationResult(isValid, warningValidator.ErrorMessages);
		}
	}
}