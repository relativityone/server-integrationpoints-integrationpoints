using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Core.Validation.Helpers;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
    public abstract class BaseExportSettingsValidator : BasePartsValidator<ExportSettings>
    {
        protected INonValidCharactersValidator NonValidCharactersValidator { get; }

        public BaseExportSettingsValidator(INonValidCharactersValidator nonValidCharactersValidator)
        {
            NonValidCharactersValidator = nonValidCharactersValidator;
        }

        public override ValidationResult Validate(ExportSettings value)
        {
            var result = new ValidationResult();

            result.Add(ValidateExportLocation(value.ExportFilesLocation));

            result.Add(ValidateLoadFile(value));

            result.Add(ValidateImages(value));

            result.Add(ValidateNatives(value));

            result.Add(ValidateTextFieldsAsFiles(value));

            return result;
        }

        internal ValidationResult ValidateExportLocation(string location)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(location))
            {
                result.Add(FileDestinationProviderValidationMessages.SETTINGS_UNKNOWN_LOCATION);
            }

            return result;
        }

        internal ValidationResult ValidateLoadFile(ExportSettings value)
        {
            var result = new ValidationResult();

            // 'OutputDataFileFormat' - no need to explicitly validate as it must be set to proper value already

            if (value.DataFileEncoding == null)
            {
                result.Add(FileDestinationProviderValidationMessages.SETTINGS_LOADFILE_UNKNOWN_ENCODING);
            }

            // 'FilePath' - no need to explicitly validate as it must be set to proper value already

            return result;
        }

        internal virtual ValidationResult ValidateImages(ExportSettings value)
        {
            return new ValidationResult();
        }

        internal virtual ValidationResult ValidateNatives(ExportSettings value)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(value.SubdirectoryNativePrefix))
            {
                result.Add(FileDestinationProviderValidationMessages.SETTINGS_NATIVES_UNKNOWN_SUBDIR_PREFIX);
            }
            else
            {
                string errorMessage = FileDestinationProviderValidationMessages.SETTINGS_NATIVES_PREFIX_ILLEGAL_CHARACTERS;
                ValidationResult isValidNameForDirectory = NonValidCharactersValidator.Validate(value.SubdirectoryNativePrefix, errorMessage);
                result.Add(isValidNameForDirectory);
            }

            return result;
        }

        internal virtual ValidationResult ValidateTextFieldsAsFiles(ExportSettings value)
        {
            var result = new ValidationResult();

            if (value.ExportFullTextAsFile)
            {
                if (value.TextFileEncodingType == null)
                {
                    result.Add(FileDestinationProviderValidationMessages.SETTINGS_TEXTFILES_UNKNOWN_ENCODING);
                }
                if ((value.TextPrecedenceFieldsIds == null) || (value.TextPrecedenceFieldsIds.Count == 0))
                {
                    result.Add(FileDestinationProviderValidationMessages.SETTINGS_TEXTFILES_UNKNOWN_PRECEDENCE);
                }
                if (string.IsNullOrWhiteSpace(value.SubdirectoryTextPrefix))
                {
                    result.Add(FileDestinationProviderValidationMessages.SETTINGS_TEXTFILES_UNKNOWN_SUBDIR_PREFIX);
                }
                else
                {
                    string errrorMessage = FileDestinationProviderValidationMessages.SETTINGS_TEXTFILES_PREFIX_ILLEGAL_CHARACTERS;
                    ValidationResult isValidNameForDirectory =
                        NonValidCharactersValidator.Validate(value.SubdirectoryTextPrefix, errrorMessage);
                    result.Add(isValidNameForDirectory);
                }
            }

            return result;
        }

        internal virtual ValidationResult ValidateVolumePrefix(ExportSettings value)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(value.VolumePrefix))
            {
                result.Add(FileDestinationProviderValidationMessages.SETTINGS_VOLUME_PREFIX_UNKNOWN);
            }
            else
            {
                string errorMessage = FileDestinationProviderValidationMessages.SETTINGS_VOLUME_PREFIX_ILLEGAL_CHARACTERS;
                ValidationResult isValidNameForDirectory = NonValidCharactersValidator.Validate(value.VolumePrefix, errorMessage);
                result.Add(isValidNameForDirectory);
            }

            return result;
        }
    }
}
