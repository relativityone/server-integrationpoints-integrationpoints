using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using NUnit.Framework;
using ExportStatistics = Relativity.API.Foundation.ExportStatistics;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.ExportManagers
{
    [TestFixture, Category("Unit")]
    public class ExportConvertingExtensionsTests
    {
        public void ToExportStatsTest()
        {
            const string type = "Type";
            int[] fields = { 1, 12, 42 };
            const string destinationFilesystemFolder = "DestFolder";
            const bool overwriteFiles = true;
            const string volumePrefix = "VOL_PREF";
            const int volumeMaxSize = 123;
            const string subdirectoryImagePrefix = "IMG_PREF";
            const string subdirectoryNativePrefix = "NAT_PREF";
            const string subdirectoryTextPrefix = "TEXT_PREF";
            const int subdirectoryStartNumber = 456;
            const int subdirectoryMaxFileCount = 789;
            const string filePathSettings = "SomeFilePathSettings";
            const char delimiter = '|';
            const char bound = '-';
            const char newlineProxy = '/';
            const char multiValueDelimiter = '+';
            const char nestedValueDelimiter = '=';
            const int textAndNativeFilesNamesAfterieldID = 12;
            const bool appendOriginalFilenames = true;
            const bool exportImages = true;
            const bool exportNativeFiles = true;
            const int metadataLoadFileEncodingCodePage = 345;
            const bool exportTextFieldsAsFiles = true;
            const int exportedTextFileEncodingCodePage = 678;
            const int exportedTextFieldID = 901;
            const bool exportMultipleChoiceFieldsAsNested = true;
            const int totalFileBytesExported = 234;
            const int totalMetadataBytesExported = 567;
            const int errorCount = 890;
            const int warningCount = 1234;
            const int documentExportCount = 5678;
            const int fileExportCount = 9012;
            const int runtimeInMilliseconds = 3456;
            int[] productionPrecedence = { 10, 120, 420 };
            const int dataSourceArtifactID = 7890;
            const int sourceRootFolderID = 12345;
            const bool copyFilesFromRepository = true;
            const int startExportAtDocumentNumber = 67890;
            const int volumeStartNumber = 123456;
            const int artifactTypeID = 789012;

            var exportStats = new EDDS.WebAPI.AuditManagerBase.ExportStatistics();
            exportStats.Type = type;
            exportStats.Fields = fields;
            exportStats.DestinationFilesystemFolder = destinationFilesystemFolder;
            exportStats.OverwriteFiles = overwriteFiles;
            exportStats.VolumePrefix = volumePrefix;
            exportStats.VolumeMaxSize = volumeMaxSize;
            exportStats.SubdirectoryImagePrefix = subdirectoryImagePrefix;
            exportStats.SubdirectoryNativePrefix = subdirectoryNativePrefix;
            exportStats.SubdirectoryTextPrefix = subdirectoryTextPrefix;
            exportStats.SubdirectoryStartNumber = subdirectoryStartNumber;
            exportStats.SubdirectoryMaxFileCount = subdirectoryMaxFileCount;
            exportStats.FilePathSettings = filePathSettings;
            exportStats.Delimiter = delimiter;
            exportStats.Bound = bound;
            exportStats.NewlineProxy = newlineProxy;
            exportStats.MultiValueDelimiter = multiValueDelimiter;
            exportStats.NestedValueDelimiter = nestedValueDelimiter;
            exportStats.TextAndNativeFilesNamedAfterFieldID = textAndNativeFilesNamesAfterieldID;
            exportStats.AppendOriginalFilenames = appendOriginalFilenames;
            exportStats.ExportImages = exportImages;
            exportStats.ImageLoadFileFormat = EDDS.WebAPI.AuditManagerBase.ImageLoadFileFormatType.IproFullText;
            exportStats.ImageFileType = EDDS.WebAPI.AuditManagerBase.ImageFileExportType.PDF;
            exportStats.ExportNativeFiles = exportNativeFiles;
            exportStats.MetadataLoadFileFormat = EDDS.WebAPI.AuditManagerBase.LoadFileFormat.Dat;
            exportStats.MetadataLoadFileEncodingCodePage = metadataLoadFileEncodingCodePage;
            exportStats.ExportTextFieldAsFiles = exportTextFieldsAsFiles;
            exportStats.ExportedTextFileEncodingCodePage = exportedTextFileEncodingCodePage;
            exportStats.ExportedTextFieldID = exportedTextFieldID;
            exportStats.ExportMultipleChoiceFieldsAsNested = exportMultipleChoiceFieldsAsNested;
            exportStats.TotalFileBytesExported = totalFileBytesExported;
            exportStats.TotalMetadataBytesExported = totalMetadataBytesExported;
            exportStats.ErrorCount = errorCount;
            exportStats.WarningCount = warningCount;
            exportStats.DocumentExportCount = documentExportCount;
            exportStats.FileExportCount = fileExportCount;
            exportStats.RunTimeInMilliseconds = runtimeInMilliseconds;
            exportStats.ImagesToExport = EDDS.WebAPI.AuditManagerBase.ImagesToExportType.Original;
            exportStats.ProductionPrecedence = productionPrecedence;
            exportStats.DataSourceArtifactID = dataSourceArtifactID;
            exportStats.SourceRootFolderID = sourceRootFolderID;
            exportStats.CopyFilesFromRepository = copyFilesFromRepository;
            exportStats.StartExportAtDocumentNumber = startExportAtDocumentNumber;
            exportStats.VolumeStartNumber = volumeStartNumber;
            exportStats.ArtifactTypeID = artifactTypeID;

            ExportStatistics foundationExportStats = exportStats.ToFoundationExportStatistics();

            Assert.AreEqual(type, foundationExportStats.Type);
            Assert.AreEqual(fields, foundationExportStats.Fields);
            Assert.AreEqual(destinationFilesystemFolder, foundationExportStats.DestinationFilesystemFolder);
            Assert.AreEqual(overwriteFiles, foundationExportStats.OverwriteFiles);
            Assert.AreEqual(volumePrefix, foundationExportStats.VolumePrefix);
            Assert.AreEqual(volumeMaxSize, foundationExportStats.VolumeMaxSize);
            Assert.AreEqual(subdirectoryImagePrefix, foundationExportStats.SubdirectoryImagePrefix);
            Assert.AreEqual(subdirectoryNativePrefix, foundationExportStats.SubdirectoryNativePrefix);
            Assert.AreEqual(subdirectoryTextPrefix, foundationExportStats.SubdirectoryTextPrefix);
            Assert.AreEqual(subdirectoryStartNumber, foundationExportStats.SubdirectoryStartNumber);
            Assert.AreEqual(subdirectoryMaxFileCount, foundationExportStats.SubdirectoryMaxFileCount);
            Assert.AreEqual(filePathSettings, foundationExportStats.FilePathSettings);
            Assert.AreEqual(delimiter, foundationExportStats.Delimiter);
            Assert.AreEqual(bound, foundationExportStats.Bound);
            Assert.AreEqual(newlineProxy, foundationExportStats.NewlineProxy);
            Assert.AreEqual(multiValueDelimiter, foundationExportStats.MultiValueDelimiter);
            Assert.AreEqual(nestedValueDelimiter, foundationExportStats.NestedValueDelimiter);
            Assert.AreEqual(textAndNativeFilesNamesAfterieldID, foundationExportStats.TextAndNativeFilesNamedAfterFieldID);
            Assert.AreEqual(appendOriginalFilenames, foundationExportStats.AppendOriginalFilenames);
            Assert.AreEqual(exportImages, foundationExportStats.ExportImages);
            Assert.AreEqual(ExportStatistics.ImageLoadFileFormatType.IproFullText, foundationExportStats.ImageLoadFileFormat);
            Assert.AreEqual(ExportStatistics.ImageFileExportType.PDF, foundationExportStats.ImageFileType);
            Assert.AreEqual(exportNativeFiles, foundationExportStats.ExportNativeFiles);
            Assert.AreEqual(ExportStatistics.LoadFileFormat.Dat, foundationExportStats.MetadataLoadFileFormat);
            Assert.AreEqual(metadataLoadFileEncodingCodePage, foundationExportStats.MetadataLoadFileEncodingCodePage);
            Assert.AreEqual(exportTextFieldsAsFiles, foundationExportStats.ExportTextFieldAsFiles);
            Assert.AreEqual(exportedTextFileEncodingCodePage, foundationExportStats.ExportedTextFileEncodingCodePage);
            Assert.AreEqual(exportedTextFieldID, foundationExportStats.ExportedTextFieldID);
            Assert.AreEqual(exportMultipleChoiceFieldsAsNested, foundationExportStats.ExportMultipleChoiceFieldsAsNested);
            Assert.AreEqual(totalFileBytesExported, foundationExportStats.TotalFileBytesExported);
            Assert.AreEqual(totalMetadataBytesExported, foundationExportStats.TotalMetadataBytesExported);
            Assert.AreEqual(errorCount, foundationExportStats.ErrorCount);
            Assert.AreEqual(warningCount, foundationExportStats.WarningCount);
            Assert.AreEqual(documentExportCount, foundationExportStats.DocumentExportCount);
            Assert.AreEqual(fileExportCount, foundationExportStats.FileExportCount);
            Assert.AreEqual(runtimeInMilliseconds, foundationExportStats.RunTimeInMilliseconds);
            Assert.AreEqual(ExportStatistics.ImagesToExportType.Original, foundationExportStats.ImagesToExport);
            Assert.AreEqual(productionPrecedence, foundationExportStats.ProductionPrecedence);
            Assert.AreEqual(dataSourceArtifactID, foundationExportStats.DataSourceArtifactID);
            Assert.AreEqual(sourceRootFolderID, foundationExportStats.SourceRootFolderID);
            Assert.AreEqual(copyFilesFromRepository, foundationExportStats.CopyFilesFromRepository);
            Assert.AreEqual(startExportAtDocumentNumber, foundationExportStats.StartExportAtDocumentNumber);
            Assert.AreEqual(volumeStartNumber, foundationExportStats.VolumeStartNumber);
            Assert.AreEqual(artifactTypeID, foundationExportStats.ArtifactTypeID);
        }
    }
}
