using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Create TenantID for RIP's Secret Store")]
	[RunOnce(true)]
	[Guid("09854211-85C1-4360-ADAE-CED54096D86A")]
	public class SecretStorePostWorkspaceCreateEventHandler : PostWorkspaceCreateEventHandlerBase
	{
		public override Response Execute()
		{
			var createTenantIdForSecretStore = new CreateTenantIdForSecretStoreEventHandlerWrapper
			{
				Helper = Helper
			};
			return createTenantIdForSecretStore.Execute();
		}
	}
}