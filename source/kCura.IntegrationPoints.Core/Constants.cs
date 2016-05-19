using System;
using System.Text.RegularExpressions;
using Relativity.SDK.Services.StrictMode.Translator;

namespace kCura.IntegrationPoints.Core
{
	public static class Constants
	{
		public static class IntegrationPoints
		{
			public const string APP_DOMAIN_DATA_CONNECTION_STRING = "IntegrationPointsConnectionString";
			public const string APP_DOMAIN_DATA_SYSTEM_TOKEN_PROVIDER = "SystemToken";
			public const string APP_DOMAIN_SUBSYSTEM_NAME = "IntegrationPoints";
			public const string APPLICATION_GUID_STRING = "DCF6E9D1-22B6-4DA3-98F6-41381E93C30C";
			public const string DOC_OBJ_GUID = "15C36703-74EA-4FF8-9DFB-AD30ECE7530D";
			public const string INVALID_PARAMETERS = "Invalid parameters";
			public const string JOBS_ALREADY_RUNNING = "There are other jobs currently running or awaiting execution.";
			public const string NO_PERMISSION_TO_ACCESS_SAVEDSEARCH = "The saved search is no longer accessible. Please verify your settings or create a new Integration Point.";
			public const string NO_PERMISSION_TO_EDIT_DOCUMENTS = "You do not have permission to edit documents in the current workspace. Please contact your system administrator.";
			public const string NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE = "You do not have permission to import in this workspace. Please contact your system administrator.";
			public const string NO_SOURCE_PROVIDER_SPECIFIED = "A source provider was not specified for the integration point. Please create a new integration point.";
			public const string NO_USERID = "Unable to determine the user id. Please contact your system administrator.";
			public const string RELATIVITY_CUSTOMPAGE_GUID = "dcf6e9d1-22b6-4da3-98f6-41381e93c30c";
			public const string RELATIVITY_PROVIDER_CONFIGURATION = "RelativityProviderConfiguration";
			public const string RELATIVITY_PROVIDER_GUID = kCura.IntegrationPoints.Contracts.Constants.RELATIVITY_PROVIDER_GUID;
			public const string RELATIVITY_PROVIDER_NAME = "Relativity";
			public const string RELATIVITY_PROVIDER_VIEW = "RelativityProvider";
			public const string RETRY_IS_NOT_RELATIVITY_PROVIDER = "Retries are only available for the Relativity provider.";
			public const string RETRY_NO_EXISTING_ERRORS = "The integration point cannot be retried as there are no errors to be retried.";
			public static Regex InvalidMultiChoicesValueFormat = new Regex($".*{kCura.IntegrationPoints.Contracts.Constants.MULTI_VALUE_DELIMITER}|{kCura.IntegrationPoints.Contracts.Constants.NESTED_VALUE_DELIMITER}.*", RegexOptions.Compiled);
			public static Regex InvalidMultiObjectsValueFormat = new Regex($".*{kCura.IntegrationPoints.Contracts.Constants.MULTI_VALUE_DELIMITER}.*", RegexOptions.Compiled);

			public static class IntegrationPoint
			{
				public static Guid ObjectTypeGuid = new Guid("03D4F67E-22C9-488C-BEE6-411F05C52E01");
			}
		}

		public enum SourceProvider
		{
			Other = 0,
			Relativity = 1
		}
	}
}