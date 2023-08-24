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

                sourceConfiguration.TaggingOption = destinationConfiguration.TaggingOption.ToString();
                sourceConfiguration.FolderArtifactId = destinationConfiguration.DestinationFolderArtifactId;

                integrationPointModel.SourceConfiguration = sourceConfiguration;
                integrationPointModel.DestinationConfiguration = destinationConfiguration;
            }
            catch (Exception e)
            {
                _apiLog.LogError(e, "Error occurred during Relativity Provider configuration deserialization.");
                throw new ArgumentException("Invalid configuration for Relativity Provider specified.", e);
            }
        }
    }
}
