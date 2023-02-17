
namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public interface IExporterFactory
    {
        IExporter Create(ExportDataContext exportDataContext);
    }
}
