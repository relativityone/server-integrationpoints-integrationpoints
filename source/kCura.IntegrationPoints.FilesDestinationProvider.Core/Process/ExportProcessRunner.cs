using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public class ExportProcessRunner
	{
		private readonly IExportProcessBuilder _exportProcessBuilder;

		public ExportProcessRunner(IExportProcessBuilder exportProcessBuilder)
		{
			_exportProcessBuilder = exportProcessBuilder;
		}

		public void StartWith(ExportSettings settings)
		{
			var exporter = _exportProcessBuilder.Create(settings);
			exporter.Run();
		}

		public void StartWith(ExportUsingSavedSearchSettings sourceSettings, IEnumerable<FieldMap> fieldMap, int artifactTypeId)
		{
			var imageType = default(ExportSettings.ImageFileType);
			Enum.TryParse(sourceSettings.SelectedImageFileType, true, out imageType);
			ExportSettings.DataFileFormat dataFileFormat;
			Enum.TryParse(sourceSettings.SelectedDataFileFormat, true, out dataFileFormat);
			LoadFileType.FileFormat imageDataFileFormat;
			Enum.TryParse(sourceSettings.SelectedImageDataFileFormat, true, out imageDataFileFormat);

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
				QuoteSeparator = sourceSettings.QuoteSeparator
			};

			StartWith(exportSettings);
		}
	}
}