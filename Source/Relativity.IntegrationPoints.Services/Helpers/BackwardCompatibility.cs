using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
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
            RelativityProviderSourceConfiguration sourceConfiguration;
            RelativityProviderDestinationConfiguration destinationConfiguration;
            try
            {
                if (integrationPointModel.SourceConfiguration.GetType() == typeof(string))
                {
                    sourceConfiguration = JsonConvert.DeserializeObject<RelativityProviderSourceConfiguration>((string)integrationPointModel.SourceConfiguration);
                }
                else
                {
                    sourceConfiguration = JsonConvert.DeserializeObject<RelativityProviderSourceConfiguration>(JsonConvert.SerializeObject(integrationPointModel.SourceConfiguration));

                }
                destinationConfiguration =
                    JsonConvert.DeserializeObject<RelativityProviderDestinationConfiguration>(JsonConvert.SerializeObject(integrationPointModel.DestinationConfiguration));
            }
            catch (Exception e)
            {
                _apiLog.LogError(e, "Error occurred during Relativity Provider configuration deserialization.");
                throw new ArgumentException("Invalid configuration for Relativity Provider specified.", e);
            }

            integrationPointModel.SourceConfiguration = new RelativityProviderSourceConfigurationBackwardCompatibility(sourceConfiguration, destinationConfiguration);
            integrationPointModel.DestinationConfiguration = new RelativityProviderDestinationConfigurationBackwardCompatibility(destinationConfiguration, sourceConfiguration, overwriteFieldsName);
        }
    }
}
