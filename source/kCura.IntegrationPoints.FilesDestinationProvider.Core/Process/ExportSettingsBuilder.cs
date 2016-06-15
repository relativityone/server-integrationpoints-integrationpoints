using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Contracts.Models;

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

			var exportSettings = new ExportSettings
			{
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
				SubdirectoryTextPrefix = sourceSettings.SubdirectoryTextPrefix
			};

			return exportSettings;
		}


	}
}
