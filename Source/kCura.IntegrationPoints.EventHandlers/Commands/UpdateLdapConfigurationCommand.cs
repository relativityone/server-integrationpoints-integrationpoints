using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Security;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class UpdateLdapConfigurationCommand : UpdateIntegrationPointConfigurationCommandBase
	{
		private readonly IEncryptionManager _encryptionManager;
		private readonly IIntegrationPointSerializer _integrationPointSerializer;

		public UpdateLdapConfigurationCommand(IIntegrationPointForSourceService integrationPointForSourceService, IIntegrationPointService integrationPointService,
			IEncryptionManager encryptionManager, IIntegrationPointSerializer serializer) : 
			base(integrationPointForSourceService, integrationPointService)
		{
			_encryptionManager = encryptionManager;
			_integrationPointSerializer = serializer;
		}

		protected override string SourceProviderGuid => Constants.IntegrationPoints.SourceProviders.LDAP;

		protected override IntegrationPoint ConvertIntegrationPoint(IntegrationPoint integrationPoint)
		{
			if (integrationPoint.SecuredConfiguration != null)
			{
				return null;
			}
			
			string decryptedConfiguration = _encryptionManager.Decrypt(integrationPoint.SourceConfiguration);
			LDAPSettings configuration = _integrationPointSerializer.Deserialize<LDAPSettings>(decryptedConfiguration);
			LDAPSecuredConfiguration securedConfiguration = _integrationPointSerializer.Deserialize<LDAPSecuredConfiguration>(decryptedConfiguration);
			integrationPoint.SourceConfiguration = _integrationPointSerializer.Serialize(configuration);
			integrationPoint.SecuredConfiguration = _integrationPointSerializer.Serialize(securedConfiguration);
			
			return integrationPoint;
		}
	}
}
