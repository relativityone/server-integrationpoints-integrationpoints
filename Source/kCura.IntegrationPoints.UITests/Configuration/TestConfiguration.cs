using System;
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
			ConfigName = "testvm.config";

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
			Log.Information("Relativity password: " + SharedVariables.RelativityPassword);
			Log.Information("TargetDbHost: " + SharedVariables.TargetDbHost);
			Log.Information("EddsConnectionString: " + SharedVariables.EddsConnectionString);
			Log.Information("WorkspaceConnectionStringFormat: " + SharedVariables.WorkspaceConnectionStringFormat);
			Log.Information("DatabaseUserId: " + SharedVariables.DatabaseUserId);
			Log.Information("DatabasePassword: " + SharedVariables.DatabasePassword);

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
			}
			return this;
		}

	}
}
