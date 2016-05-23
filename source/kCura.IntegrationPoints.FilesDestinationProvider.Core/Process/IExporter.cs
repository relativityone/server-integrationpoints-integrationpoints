using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public interface IExporter : IExporterStatusNotification
	{
		IUserNotification InteractionManager { get; set; }
		bool ExportSearch();
	}
}