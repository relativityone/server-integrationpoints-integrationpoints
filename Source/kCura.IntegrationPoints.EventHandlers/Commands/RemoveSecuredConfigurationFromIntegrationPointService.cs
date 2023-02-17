using System;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class RemoveSecuredConfigurationFromIntegrationPointService : IRemoveSecuredConfigurationFromIntegrationPointService
    {
        private const string _SECURED_CONFIGURATION_PROPERTY_NAME = "SecuredConfiguration";

        public bool RemoveSecuredConfiguration(IntegrationPoint integrationPoint)
        {
            if (integrationPoint == null)
            {
                return false;
            }

            string originalSourceConfiguration = integrationPoint.SourceConfiguration;
            string originalDestinationConfiguration = integrationPoint.DestinationConfiguration;

            integrationPoint.SourceConfiguration = RemoveSecuredConfigurationSettingFromJson(originalSourceConfiguration);
            integrationPoint.DestinationConfiguration = RemoveSecuredConfigurationSettingFromJson(originalDestinationConfiguration);

            return integrationPoint.SourceConfiguration != originalSourceConfiguration
                   || integrationPoint.DestinationConfiguration != originalDestinationConfiguration;
        }

        private string RemoveSecuredConfigurationSettingFromJson(string configuration)
        {
            if (string.IsNullOrEmpty(configuration))
            {
                return configuration;
            }

            try
            {
                JObject jObject = JObject.Parse(configuration);
                jObject.Remove(_SECURED_CONFIGURATION_PROPERTY_NAME);

                return JsonConvert.SerializeObject(jObject, Formatting.None, JSONHelper.GetDefaultSettings());
            }
            catch (Exception)
            {
                return configuration;
            }
        }
    }
}
