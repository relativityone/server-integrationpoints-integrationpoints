using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Services;

namespace Relativity.IntegrationPoints.SourceProviderInstaller.Internals.Converters
{
	internal static class SourceProviderExtensions
	{
		public static InstallProviderDto ToInstallProviderDto(this SourceProvider provider)
		{
			if (provider == null)
			{
				return null;
			}

			return new InstallProviderDto
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

		private static InstallProviderConfigurationDto ConvertConfiguration(SourceProviderConfiguration configuration)
		{
			if (configuration == null)
			{
				return null;
			}

			return new InstallProviderConfigurationDto
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
