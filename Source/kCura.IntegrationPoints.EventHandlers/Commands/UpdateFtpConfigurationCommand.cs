using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Security;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class UpdateFtpConfigurationCommand : UpdateEncryptedConfigurationCommand
	{
		protected override string SourceProviderGuid => Constants.IntegrationPoints.SourceProviders.FTP;

		protected override string[] PropertiesToExtract { get; } = { "username", "password" };

		public UpdateFtpConfigurationCommand(IIntegrationPointForSourceService integrationPointForSourceService, IIntegrationPointService integrationPointService,
			IEncryptionManager encryptionManager, ISplitJsonObjectService splitJsonObjectService) : 
			base(integrationPointForSourceService, integrationPointService, encryptionManager, splitJsonObjectService)
		{
		}
	}
}