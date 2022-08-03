using kCura.IntegrationPoints.Core.Models;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    public interface IExportedObjectBuilder
    {
        void SetExportedObjectIdAndName(ExportSettings exportSettings, ExportFile exportFile);
    }
}