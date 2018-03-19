using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Security;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class UpdateLdapConfigurationCommand : UpdateEncryptedConfigurationCommand
	{
		protected override string SourceProviderGuid => Constants.IntegrationPoints.SourceProviders.LDAP;

		protected override string[] PropertiesToExtract { get; } = { "userName", "password" };

		public UpdateLdapConfigurationCommand(IIntegrationPointForSourceService integrationPointForSourceService, IIntegrationPointService integrationPointService,
			IEncryptionManager encryptionManager, ISplitJsonObjectService splitJsonObjectService) :
			base(integrationPointForSourceService, integrationPointService, encryptionManager, splitJsonObjectService)
		{
		}
	}
}
