﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

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

			public static Regex InvalidMultiObjectsValueFormat = new Regex($".*{kCura.IntegrationPoints.Contracts.Constants.MULTI_VALUE_DELIMITER}.*", RegexOptions.Compiled);
			public static Regex InvalidMultiChoicesValueFormat = new Regex($".*{kCura.IntegrationPoints.Contracts.Constants.MULTI_VALUE_DELIMITER}|{kCura.IntegrationPoints.Contracts.Constants.NESTED_VALUE_DELIMITER}.*", RegexOptions.Compiled);
		}
	}
}