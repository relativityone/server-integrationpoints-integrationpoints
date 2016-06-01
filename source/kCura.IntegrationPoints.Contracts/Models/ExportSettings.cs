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
    }
}