using System;

namespace kCura.IntegrationPoints.Data
{
	public enum ArtifactPermission
	{
		View,
		Edit,
		Create	
	}

	public static class GlobalConst
	{
		public const string RELATIVITY_INTEGRATION_POINTS_AGENT_GUID = "08C0CE2D-8191-4E8F-B037-899CEAEE493D";
		public const string SCHEDULE_AGENT_QUEUE_TABLE_NAME = "ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D";
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
		public const string TEMPORARY_DOC_TABLE_SOURCE_OBJECTS = "IntegrationPoint_Relativity_DestinationWorkspace_JobHistory";
		public const string TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START = "IntegrationPoint_Relativity_JobHistoryErrors_JobStart";
		public const string TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE = "IntegrationPoint_Relativity_JobHistoryErrors_JobComplete";
		public const string TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START = "IntegrationPoint_Relativity_JobHistoryErrors_ItemStart";
		public const string TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_COMPLETE = "IntegrationPoint_Relativity_JobHistoryErrors_ItemComplete";
		public const string TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START_EXCLUDED = "IntegrationPoint_Relativity_JobHistoryErrors_ItemStart_Excluded";
		public const string TEMPORARY_JOB_HISTORY_ERROR_SAVED_SEARCH_NAME = "Temporary Retry Errors Search";
		public const string TEMPORARY_DOC_TABLE_SOURCEWORKSPACE = "IntegrationPoint_Relativity_SourceWorkspace";
		public static Guid RELATIVITY_SOURCEPROVIDER_GUID = new Guid("74A863B9-00EC-4BB7-9B3E-1E22323010C6");
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
		public const string SOURCE_OBJECT_MO_QUERY_ERROR = "Unable to query for multi-object field on Document associated with DestinationWorkspace or JobHistory object.";
		public const string SOURCE_OBJECT_MO_EXISTENCE_ERROR = "Multi-object field on Document associated with Destination Workspace or JobHistory object does not exist.";
		public const string SOURCE_OBJECT_MASS_EDIT_FAILURE = "Tagging Documents with DestinationWorkspace and JobHistory object failed - Mass Edit failure.";
		public const string JOB_HISTORY_ERROR_STATUS_QUERY_ERROR = "Unable to query for Error Status field on Job History Error.";
		public const string JOB_HISTORY_ERROR_STATUS_EXISTENCE_ERROR = "Error Status field on Job History Error does not exist.";
		public const string JOB_HISTORY_ERROR_MASS_EDIT_FAILURE = "Mass Edit Error Status in JobHistoryError object failed - Mass Edit failure.";
	}
	
	public static class JobHistoryErrorErrors
	{
		public const string JOB_HISTORY_ERROR_TEMP_TABLE_CREATION_FAILURE = "Unable to create temp table for Error Status updates.";
		public const string JOB_HISTORY_ERROR_RETRIEVE_FAILURE = "Unable to query for Job History Errors associated with provided JobHistory object ({0}).";
		public const string JOB_HISTORY_ERROR_RETRIEVE_NO_RESULTS = "No Job History Errors returned for JobHistory object ({0}): {1}.";
	}

	public static class ObjectTypeErrors
	{
		public const string OBJECT_TYPE_NO_ARTIFACT_TYPE_FOUND = "Unable to retrieve Artifact Type Id for object type {0}.";
	}

	public static class RelativityProvider
	{
		public const string ERROR_CREATE_SOURCE_CASE_FIELDS_ON_DESTINATION_CASE = "Unable to create source workspace and job fields in the destination workspace. Please contact your system administrator.";
	}

	public static class ProductionConsts
	{
		public static readonly Guid ProductionInformationTypeGuid = new Guid("11F4D584-C2A7-4E13-8458-D2C031FA40B6");
		public static readonly Guid WithNativesFieldGuid = new Guid("8C8BEC7A-1DF2-4AFC-B87B-AD2ECBE97C34");
		public static readonly Guid ImageCountFieldGuid = new Guid("D92B5B06-CDF0-44BA-B365-A2396F009C73");
		public static readonly Guid DocumentFieldGuid = new Guid("1CAA97BA-1D77-40C6-9F9A-F5EA9CEFAF38");
	}

	public static class SecretStoreConstants
	{
		public const string TENANT_ID_PREFIX = "92080CA4-4903-41B0-9E4C-4DC7DF961A8E";
	}

	public static class DocumentFieldsConstants
	{
		public static readonly Guid HasNativeFieldGuid = new Guid("E09E18F3-D0C8-4CFC-96D1-FBB350FAB3E1");
		public static readonly Guid HasImagesFieldGuid = new Guid("2BAACA72-790C-4B87-A7D8-C18C45CAC63D");
		public static readonly Guid RelativityImageCount = new Guid("D726B2D9-4192-43DF-86EF-27D36560931A");

		public const int HAS_IMAGES_YES_ARTIFACT_ID = 1034243;
	}
}
