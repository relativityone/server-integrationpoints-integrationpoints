using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Utils;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.EventHandlers.Commands.RenameCustodianToEntity
{
    internal class RenameCustodianToEntityForSourceProviderCommand : UpdateIntegrationPointConfigurationCommandBase
    {
        private const string _OLD_PROPERTY_NAME = "CustodianManagerFieldContainsLink";
        private const string _NEW_PROPERTY_NAME = "EntityManagerFieldContainsLink";

        private const string _DESTINATION_CONFIGURATION = IntegrationPointFields.DestinationConfiguration;

        public RenameCustodianToEntityForSourceProviderCommand(string sourceProviderGuid, IEHHelper helper, IRelativityObjectManager relativityObjectManager) :
            base(helper, relativityObjectManager)
        {
            SourceProviderGuid = sourceProviderGuid;
        }

        protected override string SourceProviderGuid { get; }

        protected override IList<string> FieldsNamesForUpdate => new List<string>
        {
            _DESTINATION_CONFIGURATION
        };

        protected override RelativityObjectSlimDto UpdateFields(RelativityObjectSlimDto value)
        {
            string destinationConfiguration = value.FieldValues[_DESTINATION_CONFIGURATION] as string;
            if(destinationConfiguration == null)
            {
                return null;
            }

            string updatedDestinationConfiguration =
                JsonUtils.ReplacePropertyNameIfPresent(destinationConfiguration, _OLD_PROPERTY_NAME, _NEW_PROPERTY_NAME);
            if (updatedDestinationConfiguration == destinationConfiguration)
            {
                return null;
            }

            value.FieldValues[_DESTINATION_CONFIGURATION] = updatedDestinationConfiguration;
            return value;
        }
    }
}
