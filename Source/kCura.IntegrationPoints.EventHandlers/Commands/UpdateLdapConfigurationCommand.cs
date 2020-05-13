using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Security;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Commands
{
	public class UpdateLdapConfigurationCommand : UpdateEncryptedConfigurationCommand
	{
		protected override string SourceProviderGuid => Constants.IntegrationPoints.SourceProviders.LDAP;

		protected override string[] PropertiesToExtract { get; } = { "userName", "password" };

		public UpdateLdapConfigurationCommand(IEHHelper helper, IRelativityObjectManager relativityObjectManager,
			IEncryptionManager encryptionManager, ISplitJsonObjectService splitJsonObjectService)
			: base(helper, relativityObjectManager, encryptionManager, splitJsonObjectService)
		{
		}
	}
}
