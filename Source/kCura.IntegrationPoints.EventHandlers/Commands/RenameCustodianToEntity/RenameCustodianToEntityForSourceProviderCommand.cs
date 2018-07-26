using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Utils;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.Commands.RenameCustodianToEntity
{
	internal class RenameCustodianToEntityForSourceProviderCommand : UpdateIntegrationPointConfigurationCommandBase
	{
		private const string _OLD_PROPERTY_NAME = "CustodianManagerFieldContainsLink";
		private const string _NEW_PROPERTY_NAME = "EntityManagerFieldContainsLink";

		public RenameCustodianToEntityForSourceProviderCommand(string sourceProviderGuid,
			IIntegrationPointForSourceService integrationPointForSourceService, IIntegrationPointService integrationPointService) :
			base(integrationPointForSourceService, integrationPointService)
		{
			SourceProviderGuid = sourceProviderGuid;
		}

		protected override string SourceProviderGuid { get; }

		protected override IntegrationPoint ConvertIntegrationPoint(IntegrationPoint integrationPoint)
		{
			string destinationConfiguration = integrationPoint.DestinationConfiguration;
			string updatedDestinationConfiguration =
				JsonUtils.ReplacePropertyNameIfPresent(destinationConfiguration, _OLD_PROPERTY_NAME,
					_NEW_PROPERTY_NAME);

			if (updatedDestinationConfiguration == destinationConfiguration)
			{
				return null; // we don't need to save this IP - nothing changed
			}

			integrationPoint.DestinationConfiguration = updatedDestinationConfiguration;
			return integrationPoint;
		}
	}
}
