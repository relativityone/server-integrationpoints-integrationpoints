using kCura.EDDS.WebAPI.AuditManagerBase;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
    public static class ExportConvertingExtensions
    {
        public static global::Relativity.API.Foundation.ExportStatistics ToFoundationExportStatistics(this ExportStatistics exportStatistics)
        {
            if (exportStatistics == null)
            {
                return null;
            }
            var foundationExportStatistics = new global::Relativity.API.Foundation.ExportStatistics();
            foundationExportStatistics.Type = exportStatistics.Type;
            foundationExportStatistics.Fields = exportStatistics.Fields;
            foundationExportStatistics.DestinationFilesystemFolder = exportStatistics.DestinationFilesystemFolder;
            foundationExportStatistics.OverwriteFiles = exportStatistics.OverwriteFiles;
            foundationExportStatistics.VolumePrefix = exportStatistics.VolumePrefix;
            foundationExportStatistics.VolumeMaxSize = exportStatistics.VolumeMaxSize;
            foundationExportStatistics.SubdirectoryImagePrefix = exportStatistics.SubdirectoryImagePrefix;
            foundationExportStatistics.SubdirectoryNativePrefix = exportStatistics.SubdirectoryNativePrefix;
            foundationExportStatistics.SubdirectoryTextPrefix = exportStatistics.SubdirectoryTextPrefix;
            foundationExportStatistics.SubdirectoryStartNumber = exportStatistics.SubdirectoryStartNumber;
            foundationExportStatistics.SubdirectoryMaxFileCount = exportStatistics.SubdirectoryMaxFileCount;
            foundationExportStatistics.FilePathSettings = exportStatistics.FilePathSettings;
            foundationExportStatistics.Delimiter = exportStatistics.Delimiter;
            foundationExportStatistics.Bound = exportStatistics.Bound;
            foundationExportStatistics.NewlineProxy = exportStatistics.NewlineProxy;
            foundationExportStatistics.MultiValueDelimiter = exportStatistics.MultiValueDelimiter;
            foundationExportStatistics.NestedValueDelimiter = exportStatistics.NestedValueDelimiter;
            foundationExportStatistics.TextAndNativeFilesNamedAfterFieldID = exportStatistics.TextAndNativeFilesNamedAfterFieldID;
            foundationExportStatistics.AppendOriginalFilenames = exportStatistics.AppendOriginalFilenames;
            foundationExportStatistics.ExportImages = exportStatistics.ExportImages;
            foundationExportStatistics.ImageLoadFileFormat = ConvertToFoundationImageLoadFileFormatType(exportStatistics.ImageLoadFileFormat);
            foundationExportStatistics.ImageFileType = ConvertToFoundationImageFileExportType(exportStatistics.ImageFileType);
            foundationExportStatistics.ExportNativeFiles = exportStatistics.ExportNativeFiles;
            foundationExportStatistics.MetadataLoadFileFormat = ConvertToFoundationLoadFileFormat(exportStatistics.MetadataLoadFileFormat);
            foundationExportStatistics.MetadataLoadFileEncodingCodePage = exportStatistics.MetadataLoadFileEncodingCodePage;
            foundationExportStatistics.ExportTextFieldAsFiles = exportStatistics.ExportTextFieldAsFiles;
            foundationExportStatistics.ExportedTextFileEncodingCodePage = exportStatistics.ExportedTextFileEncodingCodePage;
            foundationExportStatistics.ExportedTextFieldID = exportStatistics.ExportedTextFieldID;
            foundationExportStatistics.ExportMultipleChoiceFieldsAsNested = exportStatistics.ExportMultipleChoiceFieldsAsNested;
            foundationExportStatistics.TotalFileBytesExported = exportStatistics.TotalFileBytesExported;
            foundationExportStatistics.TotalMetadataBytesExported = exportStatistics.TotalMetadataBytesExported;
            foundationExportStatistics.ErrorCount = exportStatistics.ErrorCount;
            foundationExportStatistics.WarningCount = exportStatistics.WarningCount;
            foundationExportStatistics.DocumentExportCount = exportStatistics.DocumentExportCount;
            foundationExportStatistics.FileExportCount = exportStatistics.FileExportCount;
            foundationExportStatistics.RunTimeInMilliseconds = exportStatistics.RunTimeInMilliseconds;
            foundationExportStatistics.ImagesToExport = ConvertToFoundationImagesToExportType(exportStatistics.ImagesToExport);
            foundationExportStatistics.ProductionPrecedence = exportStatistics.ProductionPrecedence;
            foundationExportStatistics.DataSourceArtifactID = exportStatistics.DataSourceArtifactID;
            foundationExportStatistics.SourceRootFolderID = exportStatistics.SourceRootFolderID;
            foundationExportStatistics.CopyFilesFromRepository = exportStatistics.CopyFilesFromRepository;
            foundationExportStatistics.StartExportAtDocumentNumber = exportStatistics.StartExportAtDocumentNumber;
            foundationExportStatistics.VolumeStartNumber = exportStatistics.VolumeStartNumber;
            foundationExportStatistics.ArtifactTypeID = exportStatistics.ArtifactTypeID;
            return foundationExportStatistics;
        }

        private static global::Relativity.API.Foundation.ExportStatistics.ImageLoadFileFormatType ConvertToFoundationImageLoadFileFormatType(ImageLoadFileFormatType imageLoadFileFormatType)
        {
            switch (imageLoadFileFormatType)
            {
                case ImageLoadFileFormatType.Opticon:
                    return global::Relativity.API.Foundation.ExportStatistics.ImageLoadFileFormatType.Opticon;
                case ImageLoadFileFormatType.Ipro:
                    return global::Relativity.API.Foundation.ExportStatistics.ImageLoadFileFormatType.Ipro;
                default:
                    return global::Relativity.API.Foundation.ExportStatistics.ImageLoadFileFormatType.IproFullText;
            }
        }

        private static global::Relativity.API.Foundation.ExportStatistics.ImageFileExportType ConvertToFoundationImageFileExportType(ImageFileExportType imageFileExportType)
        {
            switch (imageFileExportType)
            {
                case ImageFileExportType.SinglePage:
                    return global::Relativity.API.Foundation.ExportStatistics.ImageFileExportType.SinglePage;
                case ImageFileExportType.MultiPageTiff:
                    return global::Relativity.API.Foundation.ExportStatistics.ImageFileExportType.MultiPageTiff;
                default:
                    return global::Relativity.API.Foundation.ExportStatistics.ImageFileExportType.PDF;
            }
        }

        private static global::Relativity.API.Foundation.ExportStatistics.LoadFileFormat ConvertToFoundationLoadFileFormat(LoadFileFormat loadFileFormat)
        {
            switch (loadFileFormat)
            {
                case LoadFileFormat.Csv:
                    return global::Relativity.API.Foundation.ExportStatistics.LoadFileFormat.Csv;
                case LoadFileFormat.Dat:
                    return global::Relativity.API.Foundation.ExportStatistics.LoadFileFormat.Dat;
                case LoadFileFormat.Custom:
                    return global::Relativity.API.Foundation.ExportStatistics.LoadFileFormat.Custom;
                default:
                    return global::Relativity.API.Foundation.ExportStatistics.LoadFileFormat.Html;
            }
        }

        private static global::Relativity.API.Foundation.ExportStatistics.ImagesToExportType ConvertToFoundationImagesToExportType(ImagesToExportType imagesToExportType)
        {
            switch (imagesToExportType)
            {
                case ImagesToExportType.Original:
                    return global::Relativity.API.Foundation.ExportStatistics.ImagesToExportType.Original;
                case ImagesToExportType.Produced:
                    return global::Relativity.API.Foundation.ExportStatistics.ImagesToExportType.Produced;
                default:
                    return global::Relativity.API.Foundation.ExportStatistics.ImagesToExportType.Both;
            }
        }
    }
}