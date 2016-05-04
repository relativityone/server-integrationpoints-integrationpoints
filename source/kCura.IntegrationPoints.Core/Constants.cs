﻿using System.Text.RegularExpressions;

namespace kCura.IntegrationPoints.Core
{
	public static class Constants
	{
		public static class IntegrationPoints
		{
			public const string APPLICATION_GUID_STRING = "DCF6E9D1-22B6-4DA3-98F6-41381E93C30C";
			public const string APP_DOMAIN_SUBSYSTEM_NAME = "IntegrationPoints";
			public const string APP_DOMAIN_DATA_SYSTEM_TOKEN_PROVIDER = "SystemToken";
			public const string APP_DOMAIN_DATA_CONNECTION_STRING = "IntegrationPointsConnectionString";

			public const string NO_PERMISSION_TO_IMPORT = "You do not have permission to push documents to the destination workspace selected. Please contact your system administrator.";
			public const string NO_USERID = "Unable to determine the user id. Please contact your system administrator.";
			public const string NO_PERMISSION_TO_EDIT_DOCUMENTS = "You do not have permission to edit documents in the current workspace. Please contact your system administrator.";

			public const string RELATIVITY_PROVIDER_GUID = "423b4d43-eae9-4e14-b767-17d629de4bb2";

			public static Regex InvalidMultiObjectsValueFormat = new Regex($".*{kCura.IntegrationPoints.Contracts.Constants.MULTI_VALUE_DELIMITER}.*", RegexOptions.Compiled);
			public static Regex InvalidMultiChoicesValueFormat = new Regex($".*{kCura.IntegrationPoints.Contracts.Constants.MULTI_VALUE_DELIMITER}|{kCura.IntegrationPoints.Contracts.Constants.NESTED_VALUE_DELIMITER}.*", RegexOptions.Compiled);
		}
	}
}