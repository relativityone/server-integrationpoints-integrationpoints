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
            expFile.FilePrefix = "";
            expFile.IdentifierColumnName = "Control Number";
            var imagePrecs = new List<Pair>();
            imagePrecs.Add(new Pair("-1", "Original"));
            expFile.ImagePrecedence = imagePrecs.ToArray();
            expFile.LogFileFormat = LoadFileType.FileFormat.Opticon;
            expFile.ObjectTypeName = "Document";
            expFile.RenameFilesToIdentifier = true;
            expFile.StartAtDocumentNumber = 0;
            expFile.TextFileEncoding = null;
            expFile.TypeOfExport = ExportFile.ExportType.ArtifactSearch;
            expFile.TypeOfExportedFilePath = ExportFile.ExportedFilePathType.Relative;
            expFile.ViewID = 0;
            expFile.VolumeDigitPadding = 8;
            expFile.VolumeInfo = new VolumeInfo();
            expFile.VolumeInfo.VolumePrefix = "VOL";
            expFile.VolumeInfo.VolumeStartNumber = 1;
            expFile.VolumeInfo.VolumeMaxSize = 650;
            
        }
    }
}