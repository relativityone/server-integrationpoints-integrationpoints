using System;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
    public interface IDestinationFolderHelper
    {
        string GetFolder(ExportSettings exportSettings);
        void CreateDestinationSubFolderIfNeeded(ExportSettings exportSettings, string path);
    }
}
