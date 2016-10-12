using System.Runtime.InteropServices;
using kCura.IntegrationPoints.SourceProviderInstaller;

namespace JsonLoader.Eventhandlers
{
	[kCura.EventHandler.CustomAttributes.Description("Uninstall JSON provider")]
	[Guid("A4AF9126-A773-4B62-B155-7B54C6BDDC19")]
	public class Uninstaller : IntegrationPointSourceProviderUninstaller
	{
		// Intentionally empty
		// Custom actions only
	}
}
