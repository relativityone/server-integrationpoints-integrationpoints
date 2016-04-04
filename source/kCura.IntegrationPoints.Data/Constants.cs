namespace kCura.IntegrationPoints.Data
{
	public static class GlobalConst
	{
		public const string RELATIVITY_INTEGRATION_POINTS_AGENT_GUID = "08C0CE2D-8191-4E8F-B037-899CEAEE493D";
		public const string Custodian = @"d216472d-a1aa-4965-8b36-367d43d4e64c";
	}

	public partial class OverwriteFieldsChoiceGuids
	{
		public static string APPEND_GUID = "998c2b04-d42e-435b-9fba-11fec836aad8";
		public static string APPEND_AND_OVERLAY_GUID = "5450ebc3-ac57-4e6a-9d28-d607bbdcf6fd";
		public static string OVERLAY_GUID = "70a1052d-93a3-4b72-9235-ac65f0d5a515";

	}
	public partial class FrequencyChoiceGuids
	{
		public static string Daily = "B8B0849B-5E92-4730-B5F4-858DC2982491";
		public static string Weekly = "A694AAA9-F374-494C-8FE9-60C1EE388B27";
		public static string Monthly = "D8E62A6F-7B0D-4F38-881F-1699EF41B4E0";
	}

	public static class Constants
	{
		public const string OBJECT_IDENTIFIER_APPENDAGE_TEXT = " [Object Identifier]";
	}

	public static class DestinationWorkspaceObject
	{
		public const string OBJECT_TYPE_GUID = "3F45E490-B4CF-4C7D-8BB6-9CA891C0C198";
		public const string DESTINATION_WORKSPACE_ARTIFACT_ID = "207E6836-2961-466B-A0D2-29974A4FAD36";
		public const string DESTINATION_WORKSPACE_NAME = "348D7394-2658-4DA4-87D0-8183824ADF98";
		public const string DESTINATION_WORKSPACE_DOCUMENTS = "94EE2BD7-76D5-4D17-99E2-04768CCE05E6";
		public const string DESTINATION_WORKSPACE_INSTANCE_NAME = "155649C0-DB15-4EE7-B449-BFDF2A54B7B5";

		public const int BATCH_SIZE = 1000; //todo: find a better place to put this, where it can be configured
	}

	public static class DestinationWorkspaceErrors
	{
		public const string UPDATE_ERROR = "Unable to update instance of Destination Workspace object";
		public const string CREATE_ERROR = "Unable to create a new instance of Destination Workspace object";
		public const string QUERY_ERROR = "Unable to query Destination Workspace RDO";
	}
}
