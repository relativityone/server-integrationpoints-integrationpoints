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
            _log.LogInformation($"{nameof(NonDocumentObjectMigrationCommand)}: Updating fields");

            string originalDestinationConfiguration = value.FieldValues[_DESTINATION_CONFIGURATION] as string;

            if (!IsDestinationProviderRelativity(originalDestinationConfiguration))
            {
                _log.LogInformation($"{nameof(NonDocumentObjectMigrationCommand)}: Destination provider is not Relativity");
                return null;
            }

            _log.LogInformation($"{nameof(NonDocumentObjectMigrationCommand)}: Updating destination config");
            string updatedDestinationConfiguration = AddDestinationArtifactTypeIdIfMissing(originalDestinationConfiguration);

            if (updatedDestinationConfiguration == originalDestinationConfiguration)
            {
                _log.LogInformation($"{nameof(NonDocumentObjectMigrationCommand)}: Destination config not changed");
                return null;
            }

            value.FieldValues[_DESTINATION_CONFIGURATION] = updatedDestinationConfiguration;

            _log.LogInformation($"{nameof(NonDocumentObjectMigrationCommand)}: Updated destination configuration: {updatedDestinationConfiguration}");
            return value;
        }

        private bool IsDestinationProviderRelativity(string destinationConfiguration)
        {
            if (string.IsNullOrWhiteSpace(destinationConfiguration))
            {
                return false;
            }

            JObject jObject = JObject.Parse(destinationConfiguration);
            string destinationProviderType = jObject.GetValue("destinationProviderType", System.StringComparison.OrdinalIgnoreCase)?.ToString().ToUpper();

            _log.LogInformation($"{nameof(NonDocumentObjectMigrationCommand)}: Destination provider type: {destinationProviderType}");

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
                _log.LogInformation($"{nameof(NonDocumentObjectMigrationCommand)}: DestinationArtifactTypeId already exists: {destinationArtifactTypeIdStr}");
                return destinationConfiguration;
            }

            string artifactTypeIdStr = jObject.GetValue(artifactTypeIdFieldName, System.StringComparison.OrdinalIgnoreCase).ToString();
            int artifactTypeId = int.Parse(artifactTypeIdStr);

            jObject[destinationArtifactTypeIdFieldName] = artifactTypeId;

            _log.LogInformation($"{nameof(NonDocumentObjectMigrationCommand)}: DestinationArtifactTypeId set to {artifactTypeId}");

            return jObject.ToString();
        }
    }
}
