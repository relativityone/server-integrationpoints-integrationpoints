using System;

namespace kCura.IntegrationPoints.Data
{
	public enum ArtifactPermission
	{
		View,
		Edit,
		Create,
		Delete
	}

	public static class GlobalConst
	{
		public const string RELATIVITY_INTEGRATION_POINTS_AGENT_GUID = "08C0CE2D-8191-4E8F-B037-899CEAEE493D";
		public const string SCHEDULE_AGENT_QUEUE_TABLE_NAME = "ScheduleAgentQueue_08C0CE2D-8191-4E8F-B037-899CEAEE493D";
	}

	public class Constants
	{
		public const int ADMIN_CASE_ID = -1;

		public const int DEFAULT_NAME_FIELD_LENGTH = 255;
		public const int NON_SYSTEM_FIELD_START_ID = 1000000;
		public const string DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME = "Integration Points";
		public const string OBJECT_IDENTIFIER_APPENDAGE_TEXT = " [Object Identifier]";
		public const string TEMPORARY_DOC_TABLE_SOURCE_OBJECTS = "IntegrationPoint_Relativity_DestinationWorkspace_JobHistory";
		public const string TEMPORARY_DOC_TABLE_SOURCEWORKSPACE = "IntegrationPoint_Relativity_SourceWorkspace";
		public const string TEMPORARY_JOB_HISTORY_ERROR_SAVED_SEARCH_NAME = "Temporary Retry Errors Search";
		public const string TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_COMPLETE = "IntegrationPoint_Relativity_JobHistoryErrors_ItemComplete";
		public const string TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START = "IntegrationPoint_Relativity_JobHistoryErrors_ItemStart";
		public const string TEMPORARY_JOB_HISTORY_ERROR_TABLE_ITEM_START_EXCLUDED = "IntegrationPoint_Relativity_JobHistoryErrors_ItemStart_Excluded";
		public const string TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_COMPLETE = "IntegrationPoint_Relativity_JobHistoryErrors_JobComplete";
		public const string TEMPORARY_JOB_HISTORY_ERROR_TABLE_JOB_START = "IntegrationPoint_Relativity_JobHistoryErrors_JobStart";
		public static readonly Guid RELATIVITY_SOURCEPROVIDER_GUID = new Guid("74A863B9-00EC-4BB7-9B3E-1E22323010C6");
	}

	public static class TaggingErrors
	{
		private const string SOURCE_OBJECT_TAGGING_ERROR_HEADER = "Tagging Documents with DestinationWorkspace and JobHistory object failed";

		public const string LINK_OBJECT_INSTANCE_ERROR = "Unable to link Destination Workspace object to Job History object";
		public const string SOURCE_OBJECT_MASS_EDIT_FAILURE = SOURCE_OBJECT_TAGGING_ERROR_HEADER + " - Mass Edit failure.";
		public const string SOURCE_OBJECT_SCRATCH_TABLE_READ_ERROR = SOURCE_OBJECT_TAGGING_ERROR_HEADER + " - DocumentsIDs read failure.";
	}

	public static class MassEditErrors
	{
		public const string JOB_HISTORY_ERROR_STATUS_QUERY_ERROR = "Unable to query for Error Status field on Job History Error.";
		public const string JOB_HISTORY_ERROR_STATUS_EXISTENCE_ERROR = "Error Status field on Job History Error does not exist.";
		public const string JOB_HISTORY_ERROR_MASS_EDIT_FAILURE = "Mass Edit Error Status in JobHistoryError object failed - Mass Edit failure.";
	}

	public static class JobHistoryErrorErrors
	{
		public const string JOB_HISTORY_ERROR_TEMP_TABLE_CREATION_FAILURE = "Unable to create temp table for Error Status updates.";
		public const string JOB_HISTORY_ERROR_RETRIEVE_FAILURE = "Unable to query for Job History Errors associated with provided JobHistory object ({0}).";
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
		public static readonly string WithNativesFieldName = "With Natives";
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
		public static readonly string HasImagesFieldName = "Has Images";
		public static readonly Guid RelativityImageCount = new Guid("D726B2D9-4192-43DF-86EF-27D36560931A");

		public static readonly Guid ControlNumberGuid = new Guid("2a3f1212-c8ca-4fa9-ad6b-f76c97f05438");
		public static readonly Guid FileIconGuid = new Guid("861295b5-5b1d-4830-89e7-77e0a7ef1c30");

		public const string EDIT_FIELD_NAME = "Edit";

		public const int HAS_IMAGES_YES_ARTIFACT_ID = 1034243;

		public const string POPUP_FILTER_TYPE_NAME = "Popup";
	}

	public static class SavedSearchFieldsConstants
	{
		public const string NAME_FIELD = "Name";
		public const string OWNER_FIELD = "Owner";
	}
}
