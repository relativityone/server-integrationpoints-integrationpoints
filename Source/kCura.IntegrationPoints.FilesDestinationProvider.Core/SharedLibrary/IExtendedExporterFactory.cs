using kCura.Windows.Process;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public interface IExtendedExporterFactory
	{
		ExtendedExporter Create(ExtendedExportFile exportFile, Controller processController, ILoadFileHeaderFormatterFactory loadFileFormatterFactory);
		IExporter Create(ExportDataContext context, IServiceFactory serviceFactory);
	}
}