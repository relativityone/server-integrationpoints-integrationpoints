using System.Collections.Generic;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Model.Serializer
{
    public class ImportSettingsForLogSerializer
    {
        private readonly HashSet<string> _propertiesToIgnore = new HashSet<string>
        {
            nameof(ImportSettings.RelativityPassword),
            nameof(ImportSettings.RelativityUsername),
            nameof(ImportSettings.FederatedInstanceCredentials),
            nameof(ImportSettings.OnBehalfOfUserId)
        };

        public string Serialize(ImportSettings importSettings)
        {

            string serialziedSettings = JsonConvert.SerializeObject(importSettings, GetSerializerSettings());
            return serialziedSettings;
        }

        private JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new SerializeContractResolverWithIgnoredProperties<ImportSettings>(_propertiesToIgnore)
            };
        }
    }
}
