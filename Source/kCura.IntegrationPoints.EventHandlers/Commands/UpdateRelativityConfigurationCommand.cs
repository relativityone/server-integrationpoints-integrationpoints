using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Relativity.API;
using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class UpdateRelativityConfigurationCommand : UpdateIntegrationPointConfigurationCommandBase
    {
        private const string _SECURED_CONFIGURATION = IntegrationPointFields.SecuredConfiguration;
        private const string _SOURCE_CONFIGURATION = IntegrationPointFields.SourceConfiguration;
        private const string _DESTINATION_CONFIGURATION = IntegrationPointFields.DestinationConfiguration;

        public UpdateRelativityConfigurationCommand(IEHHelper helper, IRelativityObjectManager relativityObjectManager)
            : base(helper, relativityObjectManager)
        {
        }

        protected override IList<string> FieldsNamesForUpdate => new List<string>
        {
            _SECURED_CONFIGURATION,
            _SOURCE_CONFIGURATION,
            _DESTINATION_CONFIGURATION
        };

        protected override RelativityObjectSlimDto UpdateFields(RelativityObjectSlimDto value)
        {
            string securedConfiguration = value.FieldValues[_SECURED_CONFIGURATION] as string;
            if (string.IsNullOrEmpty(securedConfiguration))
            {
                return null;
            }

            string originalSourceConfiguration = value.FieldValues[_SOURCE_CONFIGURATION] as string;
            string originalDestinationConfiguration = value.FieldValues[_DESTINATION_CONFIGURATION] as string;

            string updatedSourceConfiguration = RemoveSecuredConfigurationSettingFromJson(originalSourceConfiguration);
            string updatedDestinationConfiguration = RemoveSecuredConfigurationSettingFromJson(originalDestinationConfiguration);

            if(updatedSourceConfiguration == originalSourceConfiguration && updatedDestinationConfiguration == originalDestinationConfiguration)
            {
                return null;
            }

            value.FieldValues[_SOURCE_CONFIGURATION] = updatedSourceConfiguration;
            value.FieldValues[_DESTINATION_CONFIGURATION] = updatedDestinationConfiguration;

            return value;
        }

        protected override string SourceProviderGuid => Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY;

        private string RemoveSecuredConfigurationSettingFromJson(string configuration)
        {
            const string securedConfigurationPropertyName = "SecuredConfiguration";

            if (string.IsNullOrEmpty(configuration))
            {
                return configuration;
            }

            try
            {
                JObject jObject = JObject.Parse(configuration);
                jObject.Remove(securedConfigurationPropertyName);

                return JsonConvert.SerializeObject(jObject, Formatting.None, JSONHelper.GetDefaultSettings());
            }
            catch (Exception)
            {
                return configuration;
            }
        }
    }
}