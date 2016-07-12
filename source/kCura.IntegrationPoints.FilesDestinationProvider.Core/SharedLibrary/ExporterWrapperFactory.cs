using kCura.Windows.Process;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    internal class ExporterWrapperFactory : IExporterFactory
    {
        public IExporter Create(ExportFile exportFile)
        {
            return new ExporterWrapper(new Exporter(exportFile, new Controller()));
        }
    }
}