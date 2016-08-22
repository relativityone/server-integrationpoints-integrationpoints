using System.Runtime.InteropServices;
using kCura.IntegrationPoints.SourceProviderInstaller;

namespace JsonLoader.Installer
{
	[kCura.EventHandler.CustomAttributes.Description("Uninstall Json provider")]
	[Guid("A4AF9126-A773-4B62-B155-7B54C6BDDC19")]
	public class Uninstaller : IntegrationPointSourceProviderUninstaller
	{
	}
}
