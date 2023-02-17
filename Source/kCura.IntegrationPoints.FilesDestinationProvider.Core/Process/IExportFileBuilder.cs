using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    public interface IExportFileBuilder
    {
        ExtendedExportFile Create(ExportSettings exportSettings);
    }
}
