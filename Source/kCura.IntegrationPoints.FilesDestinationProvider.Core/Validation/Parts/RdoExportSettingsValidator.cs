using kCura.IntegrationPoints.Core.Validation.Helpers;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts
{
    public sealed class RdoExportSettingsValidator : BaseExportSettingsValidator
    {
        public RdoExportSettingsValidator(INonValidCharactersValidator nonValidCharactersValidator) : base(nonValidCharactersValidator)
        { }
    }
}