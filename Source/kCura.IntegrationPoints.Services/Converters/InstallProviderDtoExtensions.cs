using kCura.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Services.Converters
{
    internal static class InstallProviderDtoExtensions
    {
        public static SourceProvider ToSourceProvider(this InstallProviderDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new SourceProvider
            {
                Name = dto.Name,
                Url = dto.Url,
                ViewDataUrl = dto.ViewDataUrl,
                ApplicationID = dto.ApplicationID,
                ApplicationGUID = dto.ApplicationGUID,
                GUID = dto.GUID,
                Configuration = ConvertToSourceProviderConfiguration(dto.Configuration)
            };
        }

        private static SourceProviderConfiguration ConvertToSourceProviderConfiguration(InstallProviderConfigurationDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new SourceProviderConfiguration
            {
                AlwaysImportNativeFileNames = dto.AlwaysImportNativeFileNames,
                AlwaysImportNativeFiles = dto.AlwaysImportNativeFiles,
                CompatibleRdoTypes = dto.CompatibleRdoTypes,
                OnlyMapIdentifierToIdentifier = dto.OnlyMapIdentifierToIdentifier,
                AvailableImportSettings = new ImportSettingVisibility
                {
                    AllowUserToMapNativeFileField = dto.AllowUserToMapNativeFileField
                }
            };
        }
    }
}
