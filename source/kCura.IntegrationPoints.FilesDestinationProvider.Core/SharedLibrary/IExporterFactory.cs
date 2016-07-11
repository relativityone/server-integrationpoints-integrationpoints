using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public interface IExporterFactory
    {
        IExporter Create(ExportFile exportFile);
    }
}