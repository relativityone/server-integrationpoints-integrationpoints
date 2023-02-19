using kCura.IntegrationPoints.Core.Validation.Helpers;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
    public sealed class DocumentExportSettingsValidator : BaseExportSettingsValidator
    {
        public DocumentExportSettingsValidator(INonValidCharactersValidator nonValidCharactersValidator) : base(nonValidCharactersValidator)
        { }

        internal override ValidationResult ValidateImages(ExportSettings value)
        {
            var result = new ValidationResult();

            if (value.ExportImages)
            {
                if (!value.SelectedImageDataFileFormat.HasValue)
                {
                    result.Add(FileDestinationProviderValidationMessages.SETTINGS_IMAGES_UNKNOWN_FORMAT);
                }

                // 'ProductionPrecedence' - no need to explicitly validate as it must be set to proper value already

                if (string.IsNullOrWhiteSpace(value.SubdirectoryImagePrefix))
                {
                    result.Add(FileDestinationProviderValidationMessages.SETTINGS_IMAGES_UNKNOWN_SUBDIR_PREFIX);
                }
                else
                {
                    string errorMessage = FileDestinationProviderValidationMessages.SETTINGS_IMAGES_PREFIX_ILLEGAL_CHARACTERS;
                    ValidationResult isValidNameForDirectory =
                        NonValidCharactersValidator.Validate(value.SubdirectoryImagePrefix, errorMessage);
                    result.Add(isValidNameForDirectory);
                }
            }

            return result;
        }
    }
}
