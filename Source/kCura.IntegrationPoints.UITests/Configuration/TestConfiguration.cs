﻿using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.UITests.Logging;
using kCura.IntegrationPoint.Tests.Core;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Configuration
{
	public class TestConfiguration
	{

		private const int _LONG_RUNNING_QUERY_TIMEOUT = 100;

		private const int _DEFAULT_SQL_COMMAND_TIMEOUT_IN_SECONDS = 60 * 5;

		private const string _ENVIRONMENT_VARIABLE_CONFIG_NAME = "uiTestsConfig";

		private const string _NULL_CONFIG_NAME = "<NOT SET>";

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(TestConfiguration));
		
		public readonly string ConfigName;
	
		public TestConfiguration()
		{
			ConfigName = Environment.GetEnvironmentVariable(_ENVIRONMENT_VARIABLE_CONFIG_NAME);
			ConfigName = "Regression-B.config";

			if (ConfigName == null)
			{
				Log.Information("Custom config is not set by ENV:{ConfigName}. Only App.config defaults will be used.",
					_ENVIRONMENT_VARIABLE_CONFIG_NAME);
				ConfigName = _NULL_CONFIG_NAME;
			}
			else
			{
				Log.Information("Custom config set to {ConfigName}", ConfigName);
			}
		}

		public TestConfiguration LogConfiguration()
		{
			Log.Information("Relativity user: " + SharedVariables.RelativityUserName);
			return this;
		}

		public TestConfiguration SetupConfiguration()
		{
			kCura.Data.RowDataGateway.Config.MockConfigurationValue("LongRunningQueryTimeout", _LONG_RUNNING_QUERY_TIMEOUT);
			kCura.Data.RowDataGateway.Config.MockConfigurationValue("DefaultSqlCommandTimeout", _DEFAULT_SQL_COMMAND_TIMEOUT_IN_SECONDS);
			string connString = string.Format(SharedVariables.EddsConnectionString,
				SharedVariables.TargetDbHost, SharedVariables.DatabaseUserId, SharedVariables.DatabasePassword);
			Log.Information("connString: " + connString);
			Config.Config.SetConnectionString(connString);

			global::Relativity.Data.Config.InjectConfigSettings(new Dictionary<string, object>
			{
				{"connectionString", SharedVariables.EddsConnectionString},
			}
			);

			return this;
		}

		public TestConfiguration MergeCustomConfigWithAppSettings()
		{
			if (ConfigName == _NULL_CONFIG_NAME)
			{
				Log.Information("Custom config was not set, skipping merging.");
			}
			else
			{
				string path = $@"UiTestsConfig\{ConfigName}";
				Log.Information("Merging {Path}...", path);
				SharedVariables.MergeConfigurationWithAppConfig(path);
				Console.WriteLine(SharedVariables.DumpToString());
			}
			return this;
		}

	}
}
