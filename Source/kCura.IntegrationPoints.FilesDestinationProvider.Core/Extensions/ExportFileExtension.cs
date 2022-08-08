using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Extensions
{
    static class ExportFileExtension
    {
        internal static bool AreSettingsApplicableForProdBegBatesNameCheck(this ExportFile exportFile)
        {
            return exportFile.TypeOfExport != ExportFile.ExportType.Production &&
                    exportFile.ExportNativesToFileNamedFrom == ExportNativeWithFilenameFrom.Production;
        }
    }
}
