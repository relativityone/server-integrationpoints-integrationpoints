namespace kCura.IntegrationPoints.Contracts.Models
{
	public class ExportSettings
	{
		public int SourceWorkspaceArtifactId { set; get; }
		public int TargetWorkspaceArtifactId { get; set; }
		public string TargetWorkspace { get; set; }
		public string SourceWorkspace { get; set; }
	}

	public class ExportUsingSavedSearchSettings : ExportSettings
	{
		public int SavedSearchArtifactId { set; get; }
		public string SavedSearch { set; get; }
		public string Fileshare { get; set; }
		public bool CopyFileFromRepository { get; set; }
		public bool OverwriteFiles { get; set; }
		public bool ExportImagesChecked { get; set; }
		public string SelectedImageFileType { get; set; }
		public string SelectedDataFileFormat { get; set; }
		public bool IncludeNativeFilesPath { get; set; }
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
	}
}