using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Removes tenant and all secrets associated with RIP")]
	[RunOnce(true)]
	[Guid("CFB90708-63CB-4C9C-A0AE-BBCFCDE1E07E")]
	public class SecretStoreUninstaller : PreUninstallEventHandler
	{
		public override Response Execute()
		{
			var secretStoreCleanUp = new SecretStoreCleanUpEventHandlerWrapper
			{
				Helper = Helper
			};
			return secretStoreCleanUp.Execute();
		}
	}
}