using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    internal static class ExportFileHelper
    {
        public static void SetDefaultValues(ExportFile expFile)
        {
            expFile.ObjectTypeName = "Document";
            expFile.RenameFilesToIdentifier = true;
        }
    }
}