namespace kCura.IntegrationPoints.Domain.Models
{
	public class DestinationWorkspaceDTO
	{
		public int ArtifactId { get; set; }
		public int WorkspaceArtifactId { get; set; }
		public string WorkspaceName { get; set; }

		public static class Fields
		{
			public const string OBJECT_TYPE_GUID = "3F45E490-B4CF-4C7D-8BB6-9CA891C0C198";
			public const string DESTINATION_WORKSPACE_ARTIFACT_ID = "207E6836-2961-466B-A0D2-29974A4FAD36";
			public const string DESTINATION_WORKSPACE_NAME = "348D7394-2658-4DA4-87D0-8183824ADF98";
			public const string DESTINATION_WORKSPACE_DOCUMENTS = "94EE2BD7-76D5-4D17-99E2-04768CCE05E6";
			public const string DESTINATION_WORKSPACE_INSTANCE_NAME = "155649C0-DB15-4EE7-B449-BFDF2A54B7B5";
		}
	}
}
