using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public class ExportSettingsBuilder : IExportSettingsBuilder
	{
		public ExportSettings Create(ExportUsingSavedSearchSettings sourceSettings, IEnumerable<FieldMap> fieldMap, int artifactTypeId)
		{
			ExportSettings.ImageFileType? imageType;
			EnumHelper.TryParse(sourceSettings.SelectedImageFileType, out imageType);

			ExportSettings.DataFileFormat dataFileFormat;
			EnumHelper.Parse(sourceSettings.SelectedDataFileFormat, out dataFileFormat);

			ExportSettings.ImageDataFileFormat? imageDataFileFormat;
			EnumHelper.TryParse(sourceSettings.SelectedImageDataFileFormat, out imageDataFileFormat);

			ExportSettings.FilePathType filePath;
			EnumHelper.Parse(sourceSettings.FilePath, out filePath);

			var textFileEncoding = sourceSettings.TextFileEncodingType.IsNullOrEmpty() ? null : Encoding.GetEncoding(sourceSettings.TextFileEncodingType);

			var exportSettings = new ExportSettings
			{
				StartExportAtRecord = sourceSettings.StartExportAtRecord,
				ExportedObjArtifactId = sourceSettings.SavedSearchArtifactId,
				ExportedObjName = sourceSettings.SavedSearch,
				ExportImages = sourceSettings.ExportImagesChecked,
				ImageType = imageType,
				WorkspaceId = sourceSettings.SourceWorkspaceArtifactId,
				ExportFilesLocation = sourceSettings.Fileshare,
				OverwriteFiles = sourceSettings.OverwriteFiles,
				CopyFileFromRepository = sourceSettings.CopyFileFromRepository,
				SelViewFieldIds = fieldMap.Select(item => int.Parse(item.SourceField.FieldIdentifier)).ToList(),
				ArtifactTypeId = artifactTypeId,
				OutputDataFileFormat = dataFileFormat,
				IncludeNativeFilesPath = sourceSettings.IncludeNativeFilesPath,
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
				ExportFullTextAsFile = sourceSettings.ExportFullTextAsFile,
				TextPrecedenceFieldsIds = sourceSettings.TextPrecedenceFields.Select(x => int.Parse(x.FieldIdentifier)).ToList(),
				TextFileEncodingType = textFileEncoding
			};

			return exportSettings;
		}
	}
}