using System.Collections.Generic;
using System.Text;
using FileNaming.CustomFileNaming;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core
{
    public struct ExportSettings
    {
        public enum ImageFileType
        {
            SinglePage = 0,
            MultiPage = 1,
            Pdf = 2
        }

        public enum ImageDataFileFormat
        {
            Opticon = 0,
            IPRO = 1,
            IPRO_FullText = 2,
            None = 3
        }

        public enum DataFileFormat
        {
            Concordance = 0,
            HTML = 1,
            CSV = 2,
            Custom = 3
        }

        public enum FilePathType
        {
            Relative = 0,
            Absolute = 1,
            UserPrefix = 2
        }

        public enum ProductionPrecedenceType
        {
            Original = 0,
            Produced
        }

        public enum ExportType
        {
            Folder = 0,
            FolderAndSubfolders = 1,
            ProductionSet = 2,
            SavedSearch = 3
        }

        public enum NativeFilenameFromType
        {
            Identifier = 0,
            Production = 1,
            Custom = 2,
        }

        public ExportType TypeOfExport { get; set; }
        public int SavedSearchArtifactId { get; set; }
        public string SavedSearchName { get; set; }
        public int FolderArtifactId { get; set; }
        public int ViewId { get; set; }
        public string ViewName { get; set; }
        public int ProductionId { get; set; }
        public string ProductionName { get; set; }
        public NativeFilenameFromType? ExportNativesToFileNamedFrom { get; set; }
        public int StartExportAtRecord { get; set; }
        public bool AppendOriginalFileName { get; set; }
        public int WorkspaceId { get; set; }
        public string ExportFilesLocation { get; set; }
        public Dictionary<int, FieldEntry> SelViewFieldIds { get; set; }
        public int ArtifactTypeId { get; set; }
        public bool OverwriteFiles { get; set; }
        public bool ExportNatives { get; set; }
        public bool ExportImages { get; set; }
        public ImageFileType? ImageType { get; set; }
        public DataFileFormat OutputDataFileFormat { get; set; }
        public Encoding DataFileEncoding { get; set; }
        public ImageDataFileFormat? SelectedImageDataFileFormat { get; set; }
        public char ColumnSeparator { get; set; }
        public char QuoteSeparator { get; set; }
        public char NewlineSeparator { get; set; }
        public char MultiValueSeparator { get; set; }
        public char NestedValueSeparator { get; set; }
        public string SubdirectoryImagePrefix { get; set; }
        public string SubdirectoryNativePrefix { get; set; }
        public string SubdirectoryTextPrefix { get; set; }
        public int SubdirectoryStartNumber { get; set; }
        public int SubdirectoryDigitPadding { get; set; }
        public int SubdirectoryMaxFiles { get; set; }
        public string VolumePrefix { get; set; }
        public int VolumeStartNumber { get; set; }
        public int VolumeDigitPadding { get; set; }
        public long VolumeMaxSize { get; set; }
        public FilePathType FilePath { get; set; }
        public string UserPrefix { get; set; }
        public bool ExportMultipleChoiceFieldsAsNested { get; set; }
        public bool IncludeNativeFilesPath { get; set; }
        public bool ExportFullTextAsFile { get; set; }
        public List<int> TextPrecedenceFieldsIds { get; set; }
        public Encoding TextFileEncodingType { get; set; }
        public ProductionPrecedenceType ProductionPrecedence { get; set; }
        public bool IncludeOriginalImages { get; set; }
        public IEnumerable<ProductionDTO> ImagePrecedence { get; set; }
        public bool IsAutomaticFolderCreationEnabled { get; set; }
        public List<DescriptorPart> FileNameParts { get; set; }
    }
}