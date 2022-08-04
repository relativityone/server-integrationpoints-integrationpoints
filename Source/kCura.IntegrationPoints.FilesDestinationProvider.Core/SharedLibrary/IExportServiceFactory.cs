using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public interface IExportServiceFactory
    {
        IServiceFactory Create(ExportDataContext exportDataContext);
    }
}