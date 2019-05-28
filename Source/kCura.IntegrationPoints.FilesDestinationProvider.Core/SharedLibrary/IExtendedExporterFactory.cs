using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using kCura.WinEDDS.Service.Export;
using Relativity.DataExchange.Process;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public interface IExtendedExporterFactory
	{
		ExtendedExporter Create(ExtendedExportFile exportFile, ProcessContext context, ILoadFileHeaderFormatterFactory loadFileFormatterFactory);
		IExporter Create(ExportDataContext context, IServiceFactory serviceFactory);
	}
}