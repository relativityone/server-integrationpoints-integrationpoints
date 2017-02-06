using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Create TenantID for RIP's Secret Store")]
	[RunOnce(true)]
	[Guid("24A52901-3B2E-4DB7-B45F-562A64FC4386")]
	public class CreateTenantIdForSecretStoreInstaller : PostInstallEventHandler
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