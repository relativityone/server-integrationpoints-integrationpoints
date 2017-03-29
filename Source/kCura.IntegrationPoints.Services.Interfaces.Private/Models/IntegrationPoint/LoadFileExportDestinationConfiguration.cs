
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Services
{
	public class LoadFileExportDestinationConfiguration
	{
		public int ArtifactTypeId { get; set; }
		public string ExportType { get; set; }
		public string ExportNativesToFileNamedFrom { get; set; }
		public int StartExportAtRecord { get; set; }
		public bool AppendOriginalFileName { get; set; }
		public string Fileshare { get; set; }
		public bool ExportNatives { get; set; }
		public bool OverwriteFiles { get; set; }
		public bool ExportImages { get; set; }
		public string SelectedImageFileType { get; set; }
		public string SelectedDataFileFormat { get; set; }
		public string DataFileEncodingType { get; set; }
		public string SelectedImageDataFileFormat { get; set; }
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
		public int VolumeMaxSize { get; set; }
		public string FilePath { get; set; }
		public string UserPrefix { get; set; }
		public bool ExportMultipleChoiceFieldsAsNested { get; set; }
		public bool IncludeNativeFilesPath { get; set; }
		public bool ExportFullTextAsFile { get; set; }
		public IEnumerable<FieldEntry> TextPrecedenceFields { get; set; }
		public string TextFileEncodingType { get; set; }
		public string ProductionPrecedence { get; set; }
		public bool IncludeOriginalImages { get; set; }
		public IEnumerable<ProductionDTO> ImagePrecedence { get; set; }
		public bool IsAutomaticFolderCreationEnabled { get; set; }
	}
}
