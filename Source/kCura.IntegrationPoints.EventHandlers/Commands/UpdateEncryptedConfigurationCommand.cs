using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Security;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public abstract class UpdateEncryptedConfigurationCommand : UpdateIntegrationPointConfigurationCommandBase
	{
		protected readonly IEncryptionManager EncryptionManager;
		protected readonly ISplitJsonObjectService SplitJsonObjectService;

		protected abstract string[] PropertiesToExtract { get; }

		protected UpdateEncryptedConfigurationCommand(IIntegrationPointForSourceService integrationPointForSourceService, IIntegrationPointService integrationPointService,
			IEncryptionManager encryptionManager, ISplitJsonObjectService splitJsonObjectService) :
			base(integrationPointForSourceService, integrationPointService)
		{
			EncryptionManager = encryptionManager;
			SplitJsonObjectService = splitJsonObjectService;
		}

		protected override IntegrationPoint ConvertIntegrationPoint(IntegrationPoint integrationPoint)
		{
			//if integrationPoint.SecuredConfiguration is not empty, it means upgrade was made or is not necessary
			if (integrationPoint == null || !string.IsNullOrEmpty(integrationPoint.SecuredConfiguration))
			{
				return null;
			}

			string decryptedConfiguration = EncryptionManager.Decrypt(integrationPoint.SourceConfiguration);

			SplittedJsonObject splittedConfiguration = SplitJsonObjectService.Split(decryptedConfiguration, PropertiesToExtract);
			if (splittedConfiguration == null)
			{
				return null;
			}

			integrationPoint.SourceConfiguration = splittedConfiguration.JsonWithoutExtractedProperties;
			integrationPoint.SecuredConfiguration = splittedConfiguration.JsonWithExtractedProperties;

			return integrationPoint;
		}
	}
}