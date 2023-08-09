namespace Relativity.IntegrationPoints.Services
{
	public class RelativityProviderDestinationConfiguration
	{
		public int ArtifactTypeID { get; set; }

		public int DestinationArtifactTypeID { get; set; }

		public int CaseArtifactId { get; set; }

		public int DestinationFolderArtifactId { get; set; }

		public bool ImportNativeFile { get; set; }

		public bool UseFolderPathInformation { get; set; }

		public int FolderPathSourceField { get; set; }

		public string FieldOverlayBehavior { get; set; }
	}
}