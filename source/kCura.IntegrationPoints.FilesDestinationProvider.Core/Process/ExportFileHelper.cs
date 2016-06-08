using System.Collections.Generic;
using System.Text;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    internal class ExportFileHelper : IExportFileHelper
    {
        public ExportFile CreateDefaultSetup(ExportSettings exportSettings)
        {
            var expFile = new ExportFile(exportSettings.ArtifactTypeId);
            expFile.AppendOriginalFileName = false;
            expFile.ArtifactID = exportSettings.ExportedObjArtifactId;
            expFile.CaseInfo = new CaseInfo();
            expFile.CaseInfo.ArtifactID = exportSettings.WorkspaceId;
            expFile.ExportFullText = false;
            expFile.ExportImages = exportSettings.ExportImages;
            expFile.ExportFullTextAsFile = false;
            expFile.ExportNative = exportSettings.IncludeNativeFilesPath;
            expFile.ExportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Identifier;
            expFile.FilePrefix = "";
            expFile.FolderPath = exportSettings.ExportFilesLocation;
            expFile.IdentifierColumnName = "Control Number";
            var imagePrecs = new List<Pair>();
            imagePrecs.Add(new Pair("-1", "Original"));
            expFile.ImagePrecedence = imagePrecs.ToArray();
            expFile.LoadFileEncoding = exportSettings.DataFileEncoding;
            expFile.LoadFileExtension = ParseDataFileFormat(exportSettings.OutputDataFileFormat);
            expFile.LoadFileIsHtml = IsHtml(exportSettings.OutputDataFileFormat);
            expFile.LoadFilesPrefix = exportSettings.ExportedObjName;
            expFile.LogFileFormat = exportSettings.SelectedImageDataFileFormat;
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
            expFile.RecordDelimiter = exportSettings.ColumnSeparator;
            expFile.QuoteDelimiter = exportSettings.QuoteSeparator;
            expFile.NewlineDelimiter = exportSettings.NewlineSeparator;
            expFile.MultiRecordDelimiter = exportSettings.MultiValueSeparator;
            expFile.NestedValueDelimiter = exportSettings.NestedValueSeparator;

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

        private static string ParseDataFileFormat(ExportSettings.DataFileFormat dataFileFormat)
        {
            switch (dataFileFormat)
            {
                case ExportSettings.DataFileFormat.CSV:
                    return "csv";
                case ExportSettings.DataFileFormat.Concordance:
                    return "dat";
                case ExportSettings.DataFileFormat.HTML:
                    return "html";
                case ExportSettings.DataFileFormat.Custom:
                    return "txt";
                default:
                    return null;
            }
        }

        private static bool IsHtml(ExportSettings.DataFileFormat dataFileFormat)
        {
            return dataFileFormat == ExportSettings.DataFileFormat.HTML;
        }
    }
}