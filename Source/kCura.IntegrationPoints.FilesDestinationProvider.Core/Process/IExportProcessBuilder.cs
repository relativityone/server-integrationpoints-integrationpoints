
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public interface IExportProcessBuilder
	{
		IExporter Create(ExportSettings settings, Job job);
	}
}