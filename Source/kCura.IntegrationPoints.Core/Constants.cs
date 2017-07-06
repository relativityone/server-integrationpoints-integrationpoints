using System;
using System.Text.RegularExpressions;

namespace kCura.IntegrationPoints.Core
{
	public static class Constants
	{
		public static class IntegrationPoints
		{
			public const string APPLICATION_NAME = "Integration Points";
			public const string APP_DOMAIN_DATA_CONNECTION_STRING = kCura.IntegrationPoints.Domain.Constants.IntegrationPoints.APP_DOMAIN_DATA_CONNECTION_STRING;
			public const string APP_DOMAIN_DATA_SYSTEM_TOKEN_PROVIDER = "SystemToken";
			public const string APP_DOMAIN_SUBSYSTEM_NAME = kCura.IntegrationPoints.Domain.Constants.IntegrationPoints.APP_DOMAIN_SUBSYSTEM_NAME;
			public const string INTEGRATION_POINT_OBJECT_TYPE_NAME = "IntegrationPoint";
			public const string APPLICATION_GUID_STRING = kCura.IntegrationPoints.Domain.Constants.IntegrationPoints.APPLICATION_GUID_STRING;
			public const string DOC_OBJ_GUID = "15C36703-74EA-4FF8-9DFB-AD30ECE7530D";
			public const string INVALID_PARAMETERS = "Invalid parameters";
			public const string JOBS_ALREADY_RUNNING = "There are other jobs currently running or awaiting execution.";
			public const string NO_PERMISSION_TO_ACCESS_SAVEDSEARCH = "The saved search is no longer accessible. Please verify your settings or create a new Integration Point.";
			public const string NO_PERMISSION_TO_EDIT_DOCUMENTS = "User does not have permission to edit documents in this workspace.";
			public const string NO_PERMISSION_TO_EDIT_INTEGRATIONPOINT = "User does not have permission to edit the integration point.";
			public const string NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE = "User does not have permission to import in this workspace.";
			public const string NO_PERMISSION_TO_IMPORT_TARGETWORKSPACE = "User does not have permission to import in the target workspace.";
			public const string NO_SOURCE_PROVIDER_SPECIFIED = "A source provider was not specified for the integration point. Please create a new integration point.";
			public const string NO_DESTINATION_PROVIDER_SPECIFIED = "A destination provider was not specified for the integration point. Please create a new integration point.";
			public const string NO_INTEGRATION_POINT_TYPE_SPECIFIED = "Integration point type was not specified. Please create a new integration point.";
			public const string NO_USERID = "Unable to determine the user id. Please contact your system administrator.";
			public const string RELATIVITY_CUSTOMPAGE_GUID = "dcf6e9d1-22b6-4da3-98f6-41381e93c30c";
			public const string RELATIVITY_PROVIDER_CONFIGURATION = "RelativityProviderConfiguration";
			public const string RELATIVITY_PROVIDER_GUID = kCura.IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID;
			public const string RELATIVITY_PROVIDER_NAME = "Relativity";
			public const string RELATIVITY_DESTINATION_PROVIDER_GUID = DestinationProviders.RELATIVITY;
			public const string LOAD_FILE_DESTINATION_PROVIDER_GUID = DestinationProviders.LOADFILE;
			public const string FILESHARE_PROVIDER_NAME = "Load File";
			public const string RELATIVITY_PROVIDER_VIEW = "RelativityProvider";
			public const string RETRY_IS_NOT_RELATIVITY_PROVIDER = "Retries are only available for the Relativity provider.";
			public const string RETRY_ON_STOPPED_JOB = "The transfer cannot be retried because it has been stopped.";
			public const string RETRY_NO_EXISTING_ERRORS = "The integration point cannot be retried as there are no errors to be retried.";
			public const string UNABLE_TO_RETRIEVE_INTEGRATION_POINT = "Unable to retrieve Integration Point.";
			public const string UNABLE_TO_RETRIEVE_SOURCE_PROVIDER = "Unable to retrieve Source Provider.";
			public const string UNABLE_TO_RETRIEVE_DESTINATION_PROVIDER = "Unable to retrieve Destination Provider.";
			public const string UNABLE_TO_RETRIEVE_INTEGRATION_POINT_TYPE = "Unable to retrieve Integration Point Type.";
			public const string UNABLE_TO_RUN_INTEGRATION_POINT_USER_MESSAGE = "Unable to run this Integration Point. Please contact your system administrator.";
			public const string UNABLE_TO_RUN_INTEGRATION_POINT_ADMIN_ERROR_MESSAGE = "Unable to run Integration Point.";
			public const string UNABLE_TO_RETRY_INTEGRATION_POINT_USER_MESSAGE = "Unable to retry this Integration Point. Please contact your system administrator.";
			public const string UNABLE_TO_RETRY_INTEGRATION_POINT_ADMIN_ERROR_MESSAGE = "Unable to retry Integration Point.";
			public const string UNABLE_TO_SAVE_INTEGRATION_POINT_VALIDATION_FAILED = "Unable to save the Integration Point. Validation has failed.";
			public static Regex InvalidMultiChoicesValueFormat = new Regex($".*{kCura.IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER}|{kCura.IntegrationPoints.Domain.Constants.NESTED_VALUE_DELIMITER}.*", RegexOptions.Compiled);
			public static Regex InvalidMultiObjectsValueFormat = new Regex($".*{kCura.IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER}.*", RegexOptions.Compiled);

			public const string API_CONTROLLER_NAME = "IntegrationPointsAPI";

			public static class IntegrationPoint
			{
				public static Guid ObjectTypeGuid = new Guid("03D4F67E-22C9-488C-BEE6-411F05C52E01");
			}

			public static class IntegrationPointTypes
			{
				public static Guid ImportGuid = new Guid("700D94A7-014C-4C7C-B1A2-B53229E3A1C4");
				public static string ImportName = @"Import";
				public static Guid ExportGuid = new Guid("DBB2860A-5691-449B-BC4A-E18D8519EB3A");
				public static string ExportName = @"Export";
			}

			public static class PermissionErrors
			{
				public const string UNABLE_TO_SAVE_INTEGRATION_POINT_USER_MESSAGE = "Unable to save the Integration Point. Please contact your system administrator.";
				public const string UNABLE_TO_SAVE_INTEGRATION_POINT_ADMIN_MESSAGE = "Unable to save Integration Point.";
				public const string INSUFFICIENT_PERMISSIONS = "You do not have sufficient permissions. Please contact your system administrator.";
				public const string INSUFFICIENT_PERMISSIONS_REL_ERROR_MESSAGE = "User does not have sufficient Integration Point permissions.";
			    public const string UNABLE_TO_RUN = "Unable to run Integration Point. Execution contraints are invalid.";
                public const string CURRENT_WORKSPACE_NO_ACCESS = "User does not have permission to access this workspace.";
				public const string INTEGRATION_POINT_TYPE_NO_VIEW = "User does not have permission to view Integration Points.";
				public const string INTEGRATION_POINT_INSTANCE_NO_VIEW = "User does not have permission to view the Integration Point.";
				public const string INTEGRATION_POINT_TYPE_NO_EDIT = "User does not have permission to edit Integration Points.";
				public const string INTEGRATION_POINT_INSTANCE_NO_EDIT = "User does not have permission to edit this Integration Point.";
				public const string INTEGRATION_POINT_TYPE_NO_CREATE = "User does not have permission to create Integration Points.";
				public const string JOB_HISTORY_TYPE_NO_ADD = "User does not have permission to add Job History RDOs.";
				public const string MISSING_DESTINATION_RDO_PERMISSIONS = "User does not have all required destination RDO permissions. Please make sure the user has view, edit, and add permissions for the destination RDO.";
				public const string DESTINATION_WORKSPACE_NO_ACCESS = "User does not have permission to access the destination workspace.";
				public const string DESTINATION_WORKSPACE_NO_IMPORT = "User does not have permission to import in the destination workspace.";
				public const string SOURCE_WORKSPACE_NO_EXPORT = "User does not have permission to export in the source workspace.";
				public const string SAVED_SEARCH_NO_ACCESS = "The saved search is no longer available or the user does not have access.";
				public const string SAVED_SEARCH_NOT_PUBLIC = "The saved search must be public.";
				public const string PRODUCTION_NO_ACCESS = "The production is no longer available or the user does not have access.";
				public const string UNABLE_TO_EXPORT = "Job failed. Please ensure that you have valid permissions and a valid saved search.";
				public const string JOB_HISTORY_NO_VIEW = "User does not have permission to view Job History RDOs.";
				public const string JOB_HISTORY_NO_EDIT = "User does not have permission to edit Job History RDOs.";
				public const string JOB_HISTORY_ERROR_NO_VIEW = "User does not have permission to view Job History Errors RDOs.";
				public const string DESTINATION_PROVIDER_NO_VIEW = "User does not have permission to view Destination Provider RDOs.";
				public const string SOURCE_PROVIDER_NO_VIEW = "User does not have permission to view Source Provider RDOs.";
				public const string SOURCE_PROVIDER_NO_INSTANCE_VIEW = "User does not have permission to view the Source Provider RDO.";
				public const string INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_MESSAGE = "User does not have permissions to save an Integration Point.";
				public const string INTEGRATION_POINT_SAVE_FAILURE_ADMIN_ERROR_FULLTEXT_PREFIX = "User does not have the following permissions required to save an Integration Point:";
				public const string INTEGRATION_POINT_SAVE_FAILURE_USER_MESSAGE = "You do not have all required permissions to save this Integration Point. Please contact your system administrator.";
			}

			public class Telemetry
			{
				public const string TELEMETRY_CATEGORY = "Integration Points";

				public const string BUCKET_SYNC_WORKER_EXEC_DURATION_METRIC_COLLECTOR = "IntegrationPoints.Agent.Tasks.SyncWorker.Execute.Duration";
				public const string BUCKET_SYNC_MANAGER_EXEC_DURATION_METRIC_COLLECTOR = "IntegrationPoints.Agent.Tasks.SyncManager.Execute.Duration";
				public const string BUCKET_INTEGRATION_POINT_REC_SAVE_DURATION_METRIC_COLLECTOR = "IntegrationPoints.Web.Controllers.API.Update.Duration";

				public const string CUSTOM_DATA_CORRELATIONID = "CorrelationID";

				#region Export

				public const string BUCKET_EXPORT_LIB_EXEC_DURATION_METRIC_COLLECTOR = "IntegrationPoints.SharedLibrary.Export.Duration";
				public const string BUCKET_EXPORT_PUSH_KICK_OFF_IMPORT = "IntegrationPoints.ExportServiceManager.SyncData.Duration";
				public const string BUCKET_EXPORT_PUSH_TARGET_DOCUMENTS_TAGGING_IMPORT = "IntegrationPoints.ExportServiceManager.FinalizeExportServiceObservers.Duration";

				#endregion //Export

				#region Profiles

				public const string BUCKET_INTEGRATION_POINT_PROFILE_SAVE_AS_PROFILE_DURATION_METRIC_COLLECTOR = "IntegrationPointProfiles.Web.Controllers.API.SaveAsProfile.Duration";
				public const string BUCKET_INTEGRATION_POINT_PROFILE_SAVE_DURATION_METRIC_COLLECTOR = "IntegrationPointProfiles.Web.Controllers.API.Save.Duration";

				#endregion

			}

			public class SourceProviders
			{
				public const string RELATIVITY = "423B4D43-EAE9-4E14-B767-17D629DE4BB2";
				public const string FTP = "85120BC8-B2B9-4F05-99E9-DE37BB6C0E15";
				public const string LDAP = "5BF1F2C2-9670-4D6E-A3E9-DBC83DB6C232";
				public const string IMPORTLOADFILE = "548f0873-8e5e-4da6-9f27-5f9cda764636";
			}

			public class DestinationProviders
			{
				public const string RELATIVITY = "74A863B9-00EC-4BB7-9B3E-1E22323010C6";
				public const string LOADFILE = "1D3AD995-32C5-48FE-BAA5-5D97089C8F18";
			}

			public static class Validation
			{
				public const string INTEGRATION_POINT = "7C6930C3-E272-4AA2-A49C-A7DA68F16C67";
				public const string SAVE = "F5588DFB-A2AE-46B2-BE02-E1C87AD2F186";
				public const string VIEW_ERRORS = "C4BADAD9-BA4C-4C8F-9792-AE27882AEB47";
				public const string STOP = "CB71C18F-E7E1-4BCC-B4B7-8C91BEB234D2";
			}
		}

		public static class IntegrationPointProfiles
		{
			public const string API_CONTROLLER_NAME = "IntegrationPointProfilesAPI";

			public const string UNABLE_TO_RETRIEVE_INTEGRATION_POINT_PROFILE = "Unable to retrieve Integration Point Profle.";

			public static class PermissionErrors
			{
				public const string UNABLE_TO_SAVE_INTEGRATION_POINT_PROFILE_ADMIN_MESSAGE = "Unable to save Integration Point Profile.";
				public const string UNABLE_TO_SAVE_INTEGRATION_POINT_PROFILE_USER_MESSAGE = "Unable to save the Integration Point Profile. Please contact your system administrator.";
			}

			public class Validation
			{
				public const string EMAIL = "B69D1072-63EF-4C31-9857-BCE13D1B7379";				
				public const string SCHEDULE = "D036003D-32FF-4297-84D5-2C9009C559BA";
				public const string NAME = "285F3C4A-1606-4D5A-A720-E65CE70742DD";
				public const string INTEGRATION_POINT_TYPE = "CAAE365F-2CE7-4C6E-8C68-1BAF8037D717";
			}
		}

		public static class Fields
		{
			internal static string Name = "Name";
			internal static string ArtifactId = "ArtifactID";
			internal static string Choices = "Choices";
			internal static string ObjectTypeArtifactTypeId = "Object Type Artifact Type ID";
			internal static string FieldType = "Field Type";
			internal static string FieldTypeId = "Field Type ID";
			internal static string FieldTypeName = "Field Type Name";
			internal static string IsIdentifier = "Is Identifier";
			internal static string MultipleChoice = "Multiple Choice";
		}

		public enum SourceProvider
		{
			Other = 0,
			Relativity = 1
		}

		public enum DestinationProvider
		{
			Other = 0,
			Relativity = 1,
			LoadFile = 2
		}

		public static class Export
		{
			public const string TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME = "Text Precedence";
		}
	}
}