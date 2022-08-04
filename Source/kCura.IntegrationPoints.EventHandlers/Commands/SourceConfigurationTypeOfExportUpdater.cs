using System;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class SourceConfigurationTypeOfExportUpdater : ISourceConfigurationTypeOfExportUpdater
    {
        public const string TYPE_OF_EXPORT_NODE_NAME = "TypeOfExport";
        public const SourceConfiguration.ExportType DEFAULT_EXPORT_TYPE = SourceConfiguration.ExportType.SavedSearch;

        private readonly IProviderTypeService _providerTypeService;

        public SourceConfigurationTypeOfExportUpdater(IProviderTypeService providerTypeService)
        {
            _providerTypeService = providerTypeService;
        }

        public string GetCorrectedSourceConfiguration(int? sourceProviderId, int? destinationProviderId, string sourceConfiguration)
        {
            if (sourceConfiguration == null || sourceProviderId == null || destinationProviderId == null)
            {
                return null;
            }
            if (_providerTypeService.GetProviderType(sourceProviderId.Value, destinationProviderId.Value) != ProviderType.Relativity)
            {
                return null;
            }

            string result = null;
            try
            {
                JObject sourceConf = JObject.Parse(sourceConfiguration);
                if (sourceConf.SelectToken(TYPE_OF_EXPORT_NODE_NAME) == null)
                {
                    sourceConf.Add(TYPE_OF_EXPORT_NODE_NAME, JToken.FromObject(DEFAULT_EXPORT_TYPE));
                    result = sourceConf.ToString(Formatting.None);
                }
            }
            catch (Exception)
            {
                //Ignore if SourceConfigureation object isn't proper json
            }
            return result;
        }
    }
}