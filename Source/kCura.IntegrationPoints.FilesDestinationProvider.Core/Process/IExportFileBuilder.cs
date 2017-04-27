using kCura.IntegrationPoints.Core.Models;
using kCura.WinEDDS;
using kCura.WinEDDS.Core.Model.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public interface IExportFileBuilder
	{
		ExtendedExportFile Create(ExportSettings exportSettings);
	}
}