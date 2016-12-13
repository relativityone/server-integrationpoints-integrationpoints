using System;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
	public sealed class DocumentExportSettingsValidator : BaseExportSettingsValidator
	{
		internal override ValidationResult ValidateImages(IntegrationPoints.Core.Models.ExportSettings value)
		{
			var result = new ValidationResult();

			if (value.ExportImages)
			{
				if (!value.SelectedImageDataFileFormat.HasValue)
				{
					result.Add(FileDestinationProviderValidationMessages.SETTINGS_IMAGES_UNKNOWN_FORMAT);
				}

				// 'ProductionPrecedence' - no need to explicitly validate as it must be set to proper value already

				if (String.IsNullOrWhiteSpace(value.SubdirectoryImagePrefix))
				{
					result.Add(FileDestinationProviderValidationMessages.SETTINGS_IMAGES_UNKNOWN_SUBDIR_PREFIX);
				}
			}

			return result;
		}
	}
}