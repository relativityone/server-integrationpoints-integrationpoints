using System.Collections.Generic;
using System.Text;

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
			Opticon,
			IPRO,
			IPRO_FullText
		}

		public enum DataFileFormat
		{
			Concordance,
			HTML,
			CSV,
			Custom
		}

		public int ExportedObjArtifactId { get; set; }
		public string ExportedObjName { get; set; }
		public int WorkspaceId { get; set; }
		public string ExportFilesLocation { get; set; }
		public List<int> SelViewFieldIds { get; set; }
		public int ArtifactTypeId { get; set; }
		public bool OverwriteFiles { get; set; }
		public bool CopyFileFromRepository { get; set; }
		public bool ExportImages { get; set; }
		public ImageFileType? ImageType { get; set; }
		public DataFileFormat OutputDataFileFormat { get; set; }
		public bool IncludeNativeFilesPath { get; set; }
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
	}
}