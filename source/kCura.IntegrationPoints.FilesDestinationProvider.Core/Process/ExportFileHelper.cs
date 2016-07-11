using System.Collections.Generic;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    internal class ExportFileHelper
    {
        public static void SetDefaultValues(ExportFile expFile)
        {
            expFile.AppendOriginalFileName = false;
            expFile.ExportFullText = false;
            expFile.ExportFullTextAsFile = false;
            expFile.ExportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Identifier;
            expFile.IdentifierColumnName = "Control Number";
            var imagePrecs = new List<Pair>();
            imagePrecs.Add(new Pair("-1", "Original"));
            expFile.ImagePrecedence = imagePrecs.ToArray();
            expFile.ObjectTypeName = "Document";
            expFile.RenameFilesToIdentifier = true;
            expFile.TextFileEncoding = null;
            expFile.TypeOfExport = ExportFile.ExportType.ArtifactSearch;
            expFile.ViewID = 0;
        }
    }
}