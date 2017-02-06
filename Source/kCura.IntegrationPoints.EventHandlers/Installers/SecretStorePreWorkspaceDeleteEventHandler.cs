using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.Installers.Helpers;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Removes tenant and all secrets associated with RIP")]
	[RunOnce(true)]
	[Guid("725134DF-AA9A-4AAE-80FF-4CC047AF8C15")]
	public class SecretStorePreWorkspaceDeleteEventHandler : PreWorkspaceDeleteEventHandlerBase
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