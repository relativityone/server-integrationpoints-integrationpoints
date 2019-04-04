using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Services;

namespace kCura.IntegrationPoints.SourceProviderInstaller.Internals.Converters
{
    internal static class SourceProviderExtensions
    {
        public static ProviderToInstallDto ToProviderToInstallDto(this SourceProvider provider)
        {
            if (provider == null)
            {
                return null;
            }

            return new ProviderToInstallDto
            {
                Name = provider.Name,
                ApplicationGUID = provider.ApplicationGUID,
                ApplicationID = provider.ApplicationID,
                GUID = provider.GUID,
                Url = provider.Url,
                ViewDataUrl = provider.ViewDataUrl,
                Configuration = ConvertConfiguration(provider.Configuration)
            };
        }

        private static ProviderToInstallConfigurationDto ConvertConfiguration(SourceProviderConfiguration configuration)
        {
            if (configuration == null)
            {
                return null;
            }

            return new ProviderToInstallConfigurationDto
            {
                CompatibleRdoTypes = configuration.CompatibleRdoTypes,
                AlwaysImportNativeFileNames = configuration.AlwaysImportNativeFileNames,
                AlwaysImportNativeFiles = configuration.AlwaysImportNativeFiles,
                OnlyMapIdentifierToIdentifier = configuration.OnlyMapIdentifierToIdentifier,
                AllowUserToMapNativeFileField = configuration.AvailableImportSettings?.AllowUserToMapNativeFileField ?? false
            };
        }
    }
}
