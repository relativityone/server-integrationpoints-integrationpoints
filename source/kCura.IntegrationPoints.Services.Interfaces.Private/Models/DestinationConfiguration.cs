namespace kCura.IntegrationPoints.Services
{
	public class DestinationConfiguration
	{
		public string Provider { get; set; }

		public int ArtifactTypeId { get; set; }

		public int CaseArtifactId { get; set; }

		public bool ImportNativeFile { get; set; }

		public bool UseFolderPathInformation { get; set; }

		public string FieldOverlayBehavior { get; set; }

		public string ImportOverwriteMode { get; set; }
	}
}