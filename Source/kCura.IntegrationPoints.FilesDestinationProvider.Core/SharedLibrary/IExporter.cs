using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public interface IExporter : ICoreExporterStatusNotification
	{
        IUserNotification InteractionManager { get; set; }
        bool Run();
    }
}