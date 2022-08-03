using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public interface IExtendedExporterFactory
    {
        IExporter Create(ExportDataContext context, IServiceFactory serviceFactory);
    }
}