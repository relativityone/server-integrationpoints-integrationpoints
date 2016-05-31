
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public interface IExportProcessBuilder
	{
		IExporter Create(ExportSettings settings);
	}
}