using System;

namespace kCura.IntegrationPoints.Data
{
	public static class GlobalConst
	{
		public const string APPLICATION_GUID = "DCF6E9D1-22B6-4DA3-98F6-41381E93C30C";
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

	public class Constants
	{
		public const string OBJECT_IDENTIFIER_APPENDAGE_TEXT = " [Object Identifier]";
		public const string TEMPORARY_DOC_TABLE_DEST_WS = "TempRIPDocTable_DW";
		public const string TEMPORARY_DOC_TABLE_JOB_HIST = "TempRIPDocTable_JH";
		public const string TEMPORARY_DOC_TABLE_SOURCEWORKSPACE = "TempRIPDocTable_SourceWorkspace";
		public static Guid RELATIVITY_SOURCEPROVIDER_GUID = new Guid("74A863B9-00EC-4BB7-9B3E-1E22323010C6");
	}

	public static class DestinationWorkspaceObject
	{
		public const string OBJECT_TYPE_GUID = "3F45E490-B4CF-4C7D-8BB6-9CA891C0C198";
		public const string DESTINATION_WORKSPACE_ARTIFACT_ID = "207E6836-2961-466B-A0D2-29974A4FAD36";
		public const string DESTINATION_WORKSPACE_NAME = "348D7394-2658-4DA4-87D0-8183824ADF98";
		public const string DESTINATION_WORKSPACE_DOCUMENTS = "94EE2BD7-76D5-4D17-99E2-04768CCE05E6";
		public const string DESTINATION_WORKSPACE_INSTANCE_NAME = "155649C0-DB15-4EE7-B449-BFDF2A54B7B5";
	}

	public static class DocumentMultiObjectFields
	{
		public const string DESTINATION_WORKSPACE_FIELD = "8980C2FA-0D33-4686-9A97-EA9D6F0B4196";
		public const string JOB_HISTORY_FIELD = "97BC12FA-509B-4C75-8413-6889387D8EF6";
	}

	public static class RSAPIErrors
	{
		public const string UPDATE_DEST_WORKSPACE_ERROR = "Unable to update instance of Destination Workspace object";
		public const string CREATE_DEST_WORKSPACE_ERROR = "Unable to create a new instance of Destination Workspace object";
		public const string QUERY_DEST_WORKSPACE_ERROR = "Unable to query Destination Workspace instance";
		public const string LINK_OBJECT_INSTANCE_ERROR = "Unable to link Destination Workspace object to Job History object";
	}

	public static class MassEditErrors
	{
		public const string DEST_WORKSPACE_MO_QUERY_ERROR = "Unable to query for multi-object field on Document associated with DestinationWorkspace object.";
		public const string DEST_WORKSPACE_MO_EXISTENCE_ERROR = "Multi-object field on Document associated with Destination Workspace object does not exist.";
		public const string DEST_WORKSPACE_MASS_EDIT_FAILURE = "Tagging Documents with DestinationWorkspace object failed - Mass Edit failure.";
		public const string JOB_HISTORY_MO_QUERY_ERROR = "Unable to query for multi-object field on Document associated with JobHistory object.";
		public const string JOB_HISTORY_MO_EXISTENCE_ERROR = "Multi-object field on Document associated with JobHistory object does not exist.";
		public const string JOB_HISTORY_MASS_EDIT_FAILURE = "Tagging Documents with JobHistory object failed - Mass Edit failure.";
	}
}
