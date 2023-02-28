using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Repositories;
using Newtonsoft.Json.Linq;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public class NonDocumentObjectMigrationCommand : UpdateIntegrationPointConfigurationCommandBase
    {
        private const string _DESTINATION_CONFIGURATION = IntegrationPointFields.DestinationConfiguration;
        private readonly string _sourceProviderGuid = Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY;
        private readonly string _destinationProviderGuid = Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY;

        public NonDocumentObjectMigrationCommand(IEHHelper helper, IRelativityObjectManager relativityObjectManager)
            : base(helper, relativityObjectManager)
        {
        }

        protected override IList<string> FieldsNamesForUpdate => new List<string>
        {
            _DESTINATION_CONFIGURATION
        };

        protected override string SourceProviderGuid => _sourceProviderGuid;

        protected override RelativityObjectSlimDto UpdateFields(RelativityObjectSlimDto value)
        {
            try
            {
                string originalDestinationConfiguration = value.FieldValues[_DESTINATION_CONFIGURATION] as string;

                if (!IsDestinationProviderRelativity(originalDestinationConfiguration))
                {
                    return null;
                }

                string updatedDestinationConfiguration = AddDestinationArtifactTypeIdIfMissing(originalDestinationConfiguration);

                if (updatedDestinationConfiguration == originalDestinationConfiguration)
                {
                    return null;
                }

                value.FieldValues[_DESTINATION_CONFIGURATION] = updatedDestinationConfiguration;

                return value;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error while updating Destination Configuration in {eventHandlerName}", nameof(NonDocumentObjectMigrationCommand));
                return null;
            }
        }

        private bool IsDestinationProviderRelativity(string destinationConfiguration)
        {
            if (string.IsNullOrWhiteSpace(destinationConfiguration))
            {
                return false;
            }

            JObject jObject = JObject.Parse(destinationConfiguration);
            string destinationProviderType = jObject.GetValue("destinationProviderType", System.StringComparison.OrdinalIgnoreCase)?.ToString().ToUpper();

            return destinationProviderType.Equals(_destinationProviderGuid, System.StringComparison.OrdinalIgnoreCase);
        }

        private string AddDestinationArtifactTypeIdIfMissing(string destinationConfiguration)
        {
            const string destinationArtifactTypeIdFieldName = "DestinationArtifactTypeId";
            const string artifactTypeIdFieldName = "artifactTypeID";

            JObject jObject = JObject.Parse(destinationConfiguration);
            string destinationArtifactTypeIdStr = jObject.GetValue(destinationArtifactTypeIdFieldName, System.StringComparison.OrdinalIgnoreCase)?.ToString();

            if (!string.IsNullOrWhiteSpace(destinationArtifactTypeIdStr))
            {
                return destinationConfiguration;
            }

            string artifactTypeIdStr = jObject.GetValue(artifactTypeIdFieldName, System.StringComparison.OrdinalIgnoreCase).ToString();
            int artifactTypeId = int.Parse(artifactTypeIdStr);

            jObject[destinationArtifactTypeIdFieldName] = artifactTypeId;

            return jObject.ToString();
        }
    }
}
