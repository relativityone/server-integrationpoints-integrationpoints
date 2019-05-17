using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers.Factories
{
	internal interface IExportFileDownloaderFactory
	{
		IExportFileDownloader Create(ExportFile exportFile);
	}
}
