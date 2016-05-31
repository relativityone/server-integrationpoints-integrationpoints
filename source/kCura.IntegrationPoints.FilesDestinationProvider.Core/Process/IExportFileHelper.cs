using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    public interface IExportFileHelper
    {
        ExportFile CreateDefaultSetup(ExportSettings exportSettings);
    }
}