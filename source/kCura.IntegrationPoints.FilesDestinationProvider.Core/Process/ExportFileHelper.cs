using System.Collections.Generic;
using System.Text;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    internal class ExportFileHelper
    {
        public static void SetDefaultValues(ExportFile expFile)
        {
            expFile.AppendOriginalFileName = false;
            expFile.ExportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Identifier;
            var imagePrecs = new List<Pair>();
            imagePrecs.Add(new Pair("-1", "Original"));
            expFile.ImagePrecedence = imagePrecs.ToArray();
            expFile.ObjectTypeName = "Document";
            expFile.RenameFilesToIdentifier = true;
            expFile.TextFileEncoding = Encoding.UTF8;
            expFile.TypeOfExport = ExportFile.ExportType.ArtifactSearch;
            expFile.ViewID = 0;
        }
    }
}