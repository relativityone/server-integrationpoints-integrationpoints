using System.Collections.Generic;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    class ExportFileHelper
    {
        internal static ExportFile CreateDefaultSetup(ExportSettings exportSettings)
        {
            ExportFile expFile = new ExportFile(exportSettings.ArtifactTypeId);
            expFile.AppendOriginalFileName = false;
            expFile.ArtifactID = exportSettings.ExportedObjArtifactId;
            expFile.CaseInfo = new CaseInfo();
            expFile.CaseInfo.ArtifactID = exportSettings.WorkspaceId;
            expFile.ExportFullText = false;
            expFile.ExportImages = exportSettings.ExportImages;
            expFile.ExportFullTextAsFile = false;
            expFile.ExportNative = true;
            expFile.ExportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Identifier;
            expFile.FilePrefix = "";
            expFile.FolderPath = exportSettings.ExportFilesLocation;
            expFile.IdentifierColumnName = "Control Number";
            List<Pair> imagePrecs = new List<Pair>();
            imagePrecs.Add(new Pair("-1", "Original"));
            expFile.ImagePrecedence = imagePrecs.ToArray();
            expFile.LoadFileEncoding = System.Text.Encoding.Default;
            expFile.LoadFileExtension = "dat";
            expFile.LoadFileIsHtml = false;
            expFile.LoadFilesPrefix = exportSettings.ExportedObjName;
            expFile.LogFileFormat = LoadFileType.FileFormat.Opticon;
            expFile.ObjectTypeName = "Document";
            expFile.Overwrite = exportSettings.OverwriteFiles;
            expFile.RenameFilesToIdentifier = true;
            expFile.StartAtDocumentNumber = 0;
            expFile.SubdirectoryDigitPadding = 8;
            expFile.TextFileEncoding = null;
            expFile.TypeOfExport = ExportFile.ExportType.ArtifactSearch;
            expFile.TypeOfExportedFilePath = ExportFile.ExportedFilePathType.Relative;
            expFile.TypeOfImage = ParseImageFileType(exportSettings.ImageType);
            expFile.ViewID = 0;
            expFile.VolumeDigitPadding = 8;
            expFile.VolumeInfo = new VolumeInfo();
            expFile.VolumeInfo.VolumePrefix = "VOL";
            expFile.VolumeInfo.VolumeStartNumber = 1;
            expFile.VolumeInfo.VolumeMaxSize = 650;
            expFile.VolumeInfo.SubdirectoryStartNumber = 1;
            expFile.VolumeInfo.SubdirectoryMaxSize = 500;
            expFile.VolumeInfo.CopyFilesFromRepository = exportSettings.CopyFileFromRepository;

            return expFile;
        }

        private static ExportFile.ImageType? ParseImageFileType(ExportSettings.ImageFileType fileType)
        {
            switch (fileType)
            {
                case ExportSettings.ImageFileType.SinglePage:
                    return ExportFile.ImageType.SinglePage;
                case ExportSettings.ImageFileType.MultiPage:
                    return ExportFile.ImageType.MultiPageTiff;
                case ExportSettings.ImageFileType.Pdf:
                    return ExportFile.ImageType.Pdf;
                default:
                    return null;
            }
        }
    }
}
