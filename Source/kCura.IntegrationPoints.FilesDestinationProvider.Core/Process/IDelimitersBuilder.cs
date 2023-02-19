using kCura.IntegrationPoints.Core.Models;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    public interface IDelimitersBuilder
    {
        void SetDelimiters(ExportFile exportFile, ExportSettings exportSettings);
    }
}
