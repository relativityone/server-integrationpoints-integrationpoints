using kCura.IntegrationPoints.Core.Models;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    internal class VolumeInfoBuilder : IVolumeInfoBuilder
    {
        public void SetVolumeInfo(ExportSettings exportSettings, ExportFile exportFile)
        {
            exportFile.VolumeInfo = new VolumeInfo();

            SetVolumeInfo(exportSettings, exportFile.VolumeInfo);
            SetSubdirectoryInfo(exportSettings, exportFile.VolumeInfo);

            exportFile.VolumeInfo.CopyNativeFilesFromRepository = exportSettings.ExportNatives;
            exportFile.VolumeInfo.CopyImageFilesFromRepository = exportSettings.ExportImages;
        }

        private static void SetVolumeInfo(ExportSettings exportSettings, VolumeInfo volumeInfo)
        {
            volumeInfo.VolumeMaxSize = exportSettings.VolumeMaxSize;
            volumeInfo.VolumePrefix = exportSettings.VolumePrefix;
            volumeInfo.VolumeStartNumber = exportSettings.VolumeStartNumber;
        }

        private static void SetSubdirectoryInfo(ExportSettings exportSettings, VolumeInfo volumeInfo)
        {
            volumeInfo.SubdirectoryStartNumber = exportSettings.SubdirectoryStartNumber;
            volumeInfo.SubdirectoryMaxSize = exportSettings.SubdirectoryMaxFiles;
            volumeInfo.set_SubdirectoryImagePrefix(false, exportSettings.SubdirectoryImagePrefix);
            volumeInfo.set_SubdirectoryFullTextPrefix(false, exportSettings.SubdirectoryTextPrefix);
            volumeInfo.set_SubdirectoryNativePrefix(false, exportSettings.SubdirectoryNativePrefix);
        }
    }
}
