using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.IntegrationPoints.Services.Models;

namespace Relativity.IntegrationPoints.Services.Helpers
{
    public class BackwardCompatibility : IBackwardCompatibility
    {
        private readonly IProviderTypeService _providerTypeService;
        private readonly IAPILog _apiLog;

        public BackwardCompatibility(IProviderTypeService providerTypeService, IHelper helper)
        {
            _providerTypeService = providerTypeService;
            _apiLog = helper.GetLoggerFactory().GetLogger().ForContext<BackwardCompatibility>();
        }

        public void FixIncompatibilities(IntegrationPointModel integrationPointModel, string overwriteFieldsName)
        {
            var providerType = _providerTypeService.GetProviderType(integrationPointModel.SourceProvider, integrationPointModel.DestinationProvider);

            if (providerType == ProviderType.Relativity)
            {
                FixRelativityIncompatibilities(integrationPointModel, overwriteFieldsName);
            }
        }

        private void FixRelativityIncompatibilities(IntegrationPointModel integrationPointModel, string overwriteFieldsName)
        {
            try
            {
                RelativityProviderSourceConfigurationBackwardCompatibility sourceConfiguration = integrationPointModel.SourceConfiguration.GetType() == typeof(string)
                    ? JsonConvert.DeserializeObject<RelativityProviderSourceConfigurationBackwardCompatibility>((string)integrationPointModel.SourceConfiguration)
                    : JsonConvert.DeserializeObject<RelativityProviderSourceConfigurationBackwardCompatibility>(JsonConvert.SerializeObject(integrationPointModel.SourceConfiguration));
                DestinationConfiguration destinationConfiguration = integrationPointModel.DestinationConfiguration.GetType() == typeof(string)
                    ? JsonConvert.DeserializeObject<DestinationConfiguration>((string)integrationPointModel.DestinationConfiguration)
                    : JsonConvert.DeserializeObject<DestinationConfiguration>(JsonConvert.SerializeObject(integrationPointModel.DestinationConfiguration));

                UpdateSourceConfiguration(sourceConfiguration, destinationConfiguration);
                UpdateDestinationConfiguration(overwriteFieldsName, destinationConfiguration, sourceConfiguration);

                integrationPointModel.SourceConfiguration = sourceConfiguration;
                integrationPointModel.DestinationConfiguration = destinationConfiguration;
            }
            catch (Exception e)
            {
                _apiLog.LogError(e, "Error occurred during Relativity Provider configuration deserialization.");
                throw new ArgumentException("Invalid configuration for Relativity Provider specified.", e);
            }
        }

        private static void UpdateDestinationConfiguration(
            string overwriteFieldsName,
            DestinationConfiguration destinationConfiguration,
            RelativityProviderSourceConfigurationBackwardCompatibility sourceConfiguration)
        {
            Enum.TryParse(
                overwriteFieldsName.
                    Replace("/", string.Empty).
                    Replace(" ", string.Empty), true,
                out ImportOverwriteModeEnum result);
            destinationConfiguration.ImportOverwriteMode = result;
            destinationConfiguration.DestinationArtifactTypeId = destinationConfiguration.DestinationArtifactTypeId != 0
                ? destinationConfiguration.DestinationArtifactTypeId
                : destinationConfiguration.ArtifactTypeId;
            destinationConfiguration.DestinationProviderType = kCura.IntegrationPoints.Core.Constants.IntegrationPoints
                .RELATIVITY_DESTINATION_PROVIDER_GUID;
            destinationConfiguration.Provider = "relativity";
            destinationConfiguration.ExtractedTextFieldContainsFilePath = false;
            destinationConfiguration.ExtractedTextFileEncoding = "utf-16";
            destinationConfiguration.UseDynamicFolderPath = sourceConfiguration.UseDynamicFolderPath;
        }

        private static void UpdateSourceConfiguration(
            RelativityProviderSourceConfigurationBackwardCompatibility sourceConfiguration,
            DestinationConfiguration destinationConfiguration)
        {
            sourceConfiguration.TaggingOption = destinationConfiguration.TaggingOption.ToString();
            sourceConfiguration.FolderArtifactId = destinationConfiguration.DestinationFolderArtifactId;
            sourceConfiguration.TargetWorkspaceArtifactId = destinationConfiguration.CaseArtifactId;
            sourceConfiguration.ProductionImport = destinationConfiguration.ProductionImport;
        }
    }
}
