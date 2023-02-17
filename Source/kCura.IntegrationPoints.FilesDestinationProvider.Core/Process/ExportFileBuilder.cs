using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    internal class ExportFileBuilder : IExportFileBuilder
    {
        public const string ORIGINAL_PRODUCTION_PRECEDENCE_TEXT = "Original";
        public const string ORIGINAL_PRODUCTION_PRECEDENCE_VALUE_TEXT = "-1";
        private readonly IDelimitersBuilder _delimitersBuilder;
        private readonly IVolumeInfoBuilder _volumeInfoBuilder;
        private readonly IExportedObjectBuilder _exportedObjectBuilder;

        public ExportFileBuilder(IDelimitersBuilder delimitersBuilder, IVolumeInfoBuilder volumeInfoBuilder, IExportedObjectBuilder exportedObjectBuilder)
        {
            _delimitersBuilder = delimitersBuilder;
            _volumeInfoBuilder = volumeInfoBuilder;
            _exportedObjectBuilder = exportedObjectBuilder;
        }

        public ExtendedExportFile Create(ExportSettings exportSettings)
        {
            var exportFile = new ExtendedExportFile(exportSettings.ArtifactTypeId);

            ExportFileHelper.SetDefaultValues(exportFile);

            exportFile.TypeOfExport = ParseExportType(exportSettings.TypeOfExport);

            _exportedObjectBuilder.SetExportedObjectIdAndName(exportSettings, exportFile);

            SetStartDocumentNumber(exportSettings, exportFile);

            exportFile.FolderPath = exportSettings.ExportFilesLocation;
            exportFile.Overwrite = exportSettings.OverwriteFiles;

            _volumeInfoBuilder.SetVolumeInfo(exportSettings, exportFile);

            SetCaseInfo(exportSettings, exportFile);
            SetMetadataFileSettings(exportSettings, exportFile);
            SetDigitPaddings(exportSettings, exportFile);
            SetImagesSettings(exportSettings, exportFile);

            exportFile.TypeOfExportedFilePath = ParseFilePath(exportSettings.FilePath);
            exportFile.FilePrefix = exportSettings.UserPrefix;

            exportFile.AppendOriginalFileName = exportSettings.AppendOriginalFileName;
            exportFile.ExportNative = exportSettings.ExportNatives || exportSettings.IncludeNativeFilesPath;
            exportFile.MulticodesAsNested = exportSettings.ExportMultipleChoiceFieldsAsNested;
            exportFile.ExportFullTextAsFile = exportSettings.ExportFullTextAsFile;
            exportFile.TextFileEncoding = exportSettings.TextFileEncodingType;

            _delimitersBuilder.SetDelimiters(exportFile, exportSettings);
            SetImagePrecedence(exportSettings, exportFile);

            return exportFile;
        }

        private void SetImagePrecedence(ExportSettings exportSettings, ExportFile exportFile)
        {
            switch (exportFile.TypeOfExport)
            {
                case ExportFile.ExportType.Production:
                    exportFile.ImagePrecedence = GetProductionImagePrecedenceList(exportFile).ToArray();
                    break;
                default:
                    exportFile.ImagePrecedence = GetDeafultImagePrecedenceList(exportSettings).ToArray();
                    break;
            }
        }

        private IEnumerable<Pair> GetDeafultImagePrecedenceList(ExportSettings exportSettings)
        {
            if (exportSettings.ProductionPrecedence == ExportSettings.ProductionPrecedenceType.Produced)
            {
                foreach (var productionPrecedence in exportSettings.ImagePrecedence)
                {
                    yield return new Pair(productionPrecedence.ArtifactID, productionPrecedence.DisplayName);
                }
            }
            if ((exportSettings.ProductionPrecedence == ExportSettings.ProductionPrecedenceType.Original) || exportSettings.IncludeOriginalImages)
            {
                yield return new Pair(ORIGINAL_PRODUCTION_PRECEDENCE_VALUE_TEXT, ORIGINAL_PRODUCTION_PRECEDENCE_TEXT);
            }
        }

        private IEnumerable<Pair> GetProductionImagePrecedenceList(ExportFile exportFile)
        {
            yield return new Pair(exportFile.ArtifactID.ToString(), string.Empty);
        }

        private static void SetStartDocumentNumber(ExportSettings exportSettings, ExportFile exportFile)
        {
            exportFile.StartAtDocumentNumber = exportSettings.StartExportAtRecord - 1;
        }

        private static void SetCaseInfo(ExportSettings exportSettings, ExportFile exportFile)
        {
            exportFile.CaseInfo = new global::Relativity.DataExchange.Service.CaseInfo {ArtifactID = exportSettings.WorkspaceId};
        }

        private static void SetMetadataFileSettings(ExportSettings exportSettings, ExportFile exportFile)
        {
            exportFile.LoadFileEncoding = exportSettings.DataFileEncoding;
            exportFile.LoadFileExtension = ParseDataFileFormat(exportSettings.OutputDataFileFormat);
            exportFile.LoadFileIsHtml = IsHtml(exportSettings.OutputDataFileFormat);
        }

        private void SetDigitPaddings(ExportSettings exportSettings, ExportFile exportFile)
        {
            if (exportSettings.SubdirectoryDigitPadding < 0)
            {
                throw new ArgumentException("Subdirectory Digit Padding must be non-negative number");
            }
            if (exportSettings.VolumeDigitPadding < 0)
            {
                throw new ArgumentException("Volume Digit Padding must be non-negative number");
            }
            exportFile.SubdirectoryDigitPadding = exportSettings.SubdirectoryDigitPadding;
            exportFile.VolumeDigitPadding = exportSettings.VolumeDigitPadding;
        }

        private static void SetImagesSettings(ExportSettings exportSettings, ExportFile exportFile)
        {
            if (exportSettings.ExportImages || (exportSettings.SelectedImageDataFileFormat != ExportSettings.ImageDataFileFormat.None))
            {
                exportFile.ExportImages = true;
            }
            exportFile.LogFileFormat = ParseImageImageDataFileFormat(exportSettings.SelectedImageDataFileFormat);
            SetTypeOfImage(exportSettings, exportFile);
        }

        private static void SetTypeOfImage(ExportSettings exportSettings, ExportFile exportFile)
        {
            if (exportSettings.ExportImages)
            {
                exportFile.TypeOfImage = ParseImageFileType(exportSettings.ImageType);
            }
            else
            {
                exportFile.TypeOfImage = ExportFile.ImageType.SinglePage;
            }
        }

        private static ExportFile.ExportType ParseExportType(ExportSettings.ExportType exportType)
        {
            switch (exportType)
            {
                case ExportSettings.ExportType.Folder:
                    return ExportFile.ExportType.ParentSearch;
                case ExportSettings.ExportType.FolderAndSubfolders:
                    return ExportFile.ExportType.AncestorSearch;
                case ExportSettings.ExportType.ProductionSet:
                    return ExportFile.ExportType.Production;
                case ExportSettings.ExportType.SavedSearch:
                    return ExportFile.ExportType.ArtifactSearch;
                default:
                    throw new InvalidEnumArgumentException($"Unknown ExportSettings.ExportType ({exportType})");
            }
        }

        private static ExportFile.ImageType? ParseImageFileType(ExportSettings.ImageFileType? fileType)
        {
            if (!fileType.HasValue)
            {
                return null;
            }

            switch (fileType)
            {
                case ExportSettings.ImageFileType.SinglePage:
                    return ExportFile.ImageType.SinglePage;
                case ExportSettings.ImageFileType.MultiPage:
                    return ExportFile.ImageType.MultiPageTiff;
                case ExportSettings.ImageFileType.Pdf:
                    return ExportFile.ImageType.Pdf;
                default:
                    throw new InvalidEnumArgumentException($"Unknown ExportSettings.ImageFileType ({fileType})");
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
                    throw new InvalidEnumArgumentException($"Unknown ExportSettings.DataFileFormat ({dataFileFormat})");
            }
        }

        private static bool IsHtml(ExportSettings.DataFileFormat dataFileFormat)
        {
            return dataFileFormat == ExportSettings.DataFileFormat.HTML;
        }

        private static LoadFileType.FileFormat? ParseImageImageDataFileFormat(ExportSettings.ImageDataFileFormat? imageDataFileFormat)
        {
            if (!imageDataFileFormat.HasValue)
            {
                return null;
            }

            switch (imageDataFileFormat)
            {
                case ExportSettings.ImageDataFileFormat.None:
                case ExportSettings.ImageDataFileFormat.Opticon:
                    return LoadFileType.FileFormat.Opticon;
                case ExportSettings.ImageDataFileFormat.IPRO:
                    return LoadFileType.FileFormat.IPRO;
                case ExportSettings.ImageDataFileFormat.IPRO_FullText:
                    return LoadFileType.FileFormat.IPRO_FullText;
                default:
                    throw new InvalidEnumArgumentException($"Unknown ExportSettings.ImageDataFileFormat ({imageDataFileFormat})");
            }
        }

        private static ExportFile.ExportedFilePathType ParseFilePath(ExportSettings.FilePathType filePath)
        {
            switch (filePath)
            {
                case ExportSettings.FilePathType.Relative:
                    return ExportFile.ExportedFilePathType.Relative;
                case ExportSettings.FilePathType.Absolute:
                    return ExportFile.ExportedFilePathType.Absolute;
                case ExportSettings.FilePathType.UserPrefix:
                    return ExportFile.ExportedFilePathType.Prefix;
                default:
                    throw new InvalidEnumArgumentException($"Unknown ExportSettings.FilePathType ({filePath})");
            }
        }
    }
}
