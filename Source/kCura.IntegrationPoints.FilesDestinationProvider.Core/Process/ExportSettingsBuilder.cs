using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers.FileNaming;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    public class ExportSettingsBuilder : IExportSettingsBuilder
    {
        private readonly IAPILog _logger;
        private readonly IDescriptorPartsBuilder _descriptorPartsBuilder;

        public ExportSettingsBuilder(IHelper helper, IDescriptorPartsBuilder descriptorPartsBuilder)
        {
            _descriptorPartsBuilder = descriptorPartsBuilder;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<ExportSettingsBuilder>();
        }

        public ExportSettings Create(ExportUsingSavedSearchSettings sourceSettings, IEnumerable<FieldMap> fieldMap, int artifactTypeId)
        {
            try
            {
                return CreateExportSettings(sourceSettings, fieldMap, artifactTypeId);
            }
            catch (System.Exception e)
            {
                LogCreatingExportSettingsError(e);
                throw;
            }
        }

        private ExportSettings CreateExportSettings(ExportUsingSavedSearchSettings sourceSettings, IEnumerable<FieldMap> fieldMap, int artifactTypeId)
        {
            ExportSettings.ImageFileType? imageType;
            EnumHelper.TryParse(sourceSettings.SelectedImageFileType, out imageType);

            ExportSettings.DataFileFormat dataFileFormat;
            EnumHelper.Parse(sourceSettings.SelectedDataFileFormat, out dataFileFormat);

            ExportSettings.ImageDataFileFormat imageDataFileFormat;
            EnumHelper.Parse(sourceSettings.SelectedImageDataFileFormat, out imageDataFileFormat);

            ExportSettings.FilePathType filePath;
            EnumHelper.Parse(sourceSettings.FilePath, out filePath);

            ExportSettings.ProductionPrecedenceType productionPrecedence;
            EnumHelper.Parse(sourceSettings.ProductionPrecedence, out productionPrecedence);

            ExportSettings.ExportType exportType;
            EnumHelper.Parse(sourceSettings.ExportType, out exportType);

            ExportSettings.NativeFilenameFromType? nativeFilenameFromType;
            EnumHelper.TryParse(sourceSettings.ExportNativesToFileNamedFrom, out nativeFilenameFromType);

            var textFileEncoding = sourceSettings.TextFileEncodingType.IsNullOrEmpty() ? null : Encoding.GetEncoding(sourceSettings.TextFileEncodingType);

            var exportSettings = new ExportSettings
            {
                TypeOfExport = exportType,
                StartExportAtRecord = sourceSettings.StartExportAtRecord,
                SavedSearchArtifactId = sourceSettings.SavedSearchArtifactId,
                SavedSearchName = sourceSettings.SavedSearch,
                ViewId = sourceSettings.ViewId,
                ViewName = sourceSettings.ViewName,
                FolderArtifactId = sourceSettings.FolderArtifactId,
                ExportImages = sourceSettings.ExportImages,
                ImageType = imageType,
                WorkspaceId = sourceSettings.SourceWorkspaceArtifactId,
                ExportFilesLocation = sourceSettings.Fileshare,
                OverwriteFiles = sourceSettings.OverwriteFiles,
                ExportNatives = sourceSettings.ExportNatives,
                SelViewFieldIds = fieldMap.Select(item => item.DestinationField).ToDictionary(entry => int.Parse(entry.FieldIdentifier)),
                ArtifactTypeId = artifactTypeId,
                OutputDataFileFormat = dataFileFormat,
                DataFileEncoding = Encoding.GetEncoding(sourceSettings.DataFileEncodingType),
                SelectedImageDataFileFormat = imageDataFileFormat,
                ColumnSeparator = sourceSettings.ColumnSeparator,
                MultiValueSeparator = sourceSettings.MultiValueSeparator,
                NestedValueSeparator = sourceSettings.NestedValueSeparator,
                NewlineSeparator = sourceSettings.NewlineSeparator,
                QuoteSeparator = sourceSettings.QuoteSeparator,
                SubdirectoryMaxFiles = sourceSettings.SubdirectoryMaxFiles,
                SubdirectoryStartNumber = sourceSettings.SubdirectoryStartNumber,
                SubdirectoryDigitPadding = sourceSettings.SubdirectoryDigitPadding,
                SubdirectoryNativePrefix = sourceSettings.SubdirectoryNativePrefix,
                SubdirectoryImagePrefix = sourceSettings.SubdirectoryImagePrefix,
                SubdirectoryTextPrefix = sourceSettings.SubdirectoryTextPrefix,
                VolumeDigitPadding = sourceSettings.VolumeDigitPadding,
                VolumeMaxSize = sourceSettings.VolumeMaxSize,
                VolumeStartNumber = sourceSettings.VolumeStartNumber,
                VolumePrefix = sourceSettings.VolumePrefix,
                FilePath = filePath,
                UserPrefix = sourceSettings.UserPrefix,
                ExportMultipleChoiceFieldsAsNested = sourceSettings.ExportMultipleChoiceFieldsAsNested,
                IncludeNativeFilesPath = sourceSettings.IncludeNativeFilesPath,
                ExportFullTextAsFile = sourceSettings.ExportFullTextAsFile,
                TextPrecedenceFieldsIds = sourceSettings.TextPrecedenceFields.Select(x => int.Parse(x.FieldIdentifier)).ToList(),
                TextFileEncodingType = textFileEncoding,
                ProductionPrecedence = productionPrecedence,
                IncludeOriginalImages = sourceSettings.IncludeOriginalImages,
                ImagePrecedence = sourceSettings.ImagePrecedence,
                ExportNativesToFileNamedFrom = nativeFilenameFromType,
                ProductionId = sourceSettings.ProductionId,
                ProductionName = sourceSettings.ProductionName,
                AppendOriginalFileName = sourceSettings.AppendOriginalFileName,
                IsAutomaticFolderCreationEnabled = sourceSettings.IsAutomaticFolderCreationEnabled,
                FileNameParts = _descriptorPartsBuilder.CreateDescriptorParts(sourceSettings.FileNameParts)
            };

            return exportSettings;
        }

        #region Logging

        private void LogCreatingExportSettingsError(Exception e)
        {
            _logger.LogError(e, "Failed to build ExportSettings.");
        }

        #endregion
    }
}