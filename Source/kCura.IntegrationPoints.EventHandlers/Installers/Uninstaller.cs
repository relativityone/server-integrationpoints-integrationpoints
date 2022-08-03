using System.Runtime.InteropServices;
using Relativity.IntegrationPoints.SourceProviderInstaller;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
    [EventHandler.CustomAttributes.Description("uninstall Integration Points Entities")]
    [Guid("55B133DF-A0B6-4755-AA05-11AA2A982B46")]
    public class Uninstaller : IntegrationPointSourceProviderUninstaller
    {

    }
}
