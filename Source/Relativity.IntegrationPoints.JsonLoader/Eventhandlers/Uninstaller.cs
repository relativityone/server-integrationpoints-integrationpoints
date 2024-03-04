using System.Runtime.InteropServices;
using Relativity.IntegrationPoints.SourceProviderInstaller;

namespace Relativity.IntegrationPoints.JsonLoader.Eventhandlers
{
	[kCura.EventHandler.CustomAttributes.Description("Uninstall JSON provider")]
	[Guid("A4AF9126-A773-4B62-B155-7B54C6BDDC19")]
	public class Uninstaller : IntegrationPointSourceProviderUninstaller
	{
		// This uninstaller removes the provider from the Integration Points drop-down.
		// This class is intentionally left empty. Any custom uninstall logic must be created in its own separate PreUninstallEventHandlers or PostUninstallEventHandlers.
	}
}
