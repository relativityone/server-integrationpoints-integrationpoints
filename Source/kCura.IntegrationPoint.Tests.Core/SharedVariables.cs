using System;
using System.Configuration;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class SharedVariables
	{
		public static Configuration CustomConfig { get; set; }
		
		public static string AppSettingString(string name)
		{
			return CustomConfig?.AppSettings.Settings[name]?.Value ?? ConfigurationManager.AppSettings[name];
		}

		public static int AppSettingInt(string name)
		{
			return int.Parse(AppSettingString(name));
		}

		/// <summary>
		/// Merges settings from custom config file with settings from app.config.
		/// Custom settings take precedence before those defined in app.config.
		/// </summary>
		/// <param name="configFilePath">Path to config file, automatically prefixed by AppDomain.CurrentDomain.BaseDirectory (e.g. ./bin/x64) of given project.</param>
		public static void MergeConfigurationWithAppConfig(string configFilePath)
		{
			string configFileFullPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\{configFilePath}";
			var configFileInfo = new FileInfo(configFileFullPath);
			Assert.IsTrue(configFileInfo.Exists, "Specified config file '{0}' does not exist, full path: '{1}'.",
				configFilePath, configFileInfo.FullName);

			var map = new ExeConfigurationFileMap
			{
				ExeConfigFilename = configFileFullPath
			};

			Configuration c = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

			CustomConfig = c;
		}

		#region User Settings

		public static string RelativityUserName => AppSettingString("relativityUserName");

		public static string RelativityPassword => AppSettingString("relativityPassword");

		public static string RelativityUserFirstName => AppSettingString("relativityUserFirstName");

		public static string RelativityUserLastName => AppSettingString("relativityUserLastName");

		public static string UserFullName => $"{RelativityUserLastName}, {RelativityUserFirstName}";

        #endregion User Settings
        
        #region UI Tests Settings
        public static int UiImplicitWaitInSec => AppSettingInt("ui.implicitWaitInSec");
        public static int UiWaitForAjaxCallsInSec => AppSettingInt("ui.waitForAjaxCallsInSec");
        public static int UiWaitForPageInSec => AppSettingInt("ui.waitForPageInSec");
        #endregion UI Tests Settings
        
        #region Relativity Settings

		public static string ProtocolVersion => AppSettingString("ProtocolVersion");

		public static string RsapiClientUri => $"{ProtocolVersion}://{TargetHost}/Relativity.Services";

		public static Uri RsapiClientServiceUri => new Uri($"{RsapiClientUri}/");

		public static string RestServer => $"{ProtocolVersion}://{TargetHost}/Relativity.Rest/";

		public static Uri RestClientServiceUri => new Uri($"{RestServer}/api");

		public static string RelativityWebApiUrl => $"{ProtocolVersion}://{TargetHost}/RelativityWebAPI/";

		#endregion Relativity Settings

		#region ConnectionString Settings

		public static string TargetHost => GetTargetHost();
		public static string TargetDbHost => GetTargetDbHost();

		public static string DatabaseUserId => AppSettingString("databaseUserId");

		public static string DatabasePassword => AppSettingString("databasePassword");

		public static string EddsConnectionString => string.Format(AppSettingString("connectionStringEDDS"), TargetDbHost, DatabaseUserId, DatabasePassword);

		public static string WorkspaceConnectionStringFormat => string.Format(AppSettingString("connectionStringWorkspace"), "{0}", TargetDbHost, DatabaseUserId, DatabasePassword);

		public static int KeplerTimeout => AppSettingInt("keplerTimeout");

		#endregion ConnectionString Settings

		#region RAP File Settings

		public static string ApplicationPath => AppSettingString("applicationPath");

		public static string ApplicationRapFileName => AppSettingString("applicationRapFileName");

		public static string BuildPackagesBranchPath => Path.Combine(AppSettingString("buildPackages"), AppSettingString("branch"));

		public static string LatestRapLocationFromBuildPackages => Path.Combine(BuildPackagesBranchPath, LatestRapVersionFromBuildPackages);

		public static string LatestRapVersionFromBuildPackages => GetLatestVersion();

		public static bool UseLocalRap => bool.Parse(AppSettingString("UseLocalRAP"));

		public static string RapFileLocation => AppSettingString("LocalRAPFileLocation");

		#endregion RAP File Settings

		#region LDAP Configuration Settings

		public static string LdapConnectionPath => AppSettingString("ldapConnectionPath");

		public static string LdapUsername => AppSettingString("ldapUsername");

		public static string LdapPassword => AppSettingString("ldapPassword");

		#endregion LDAP Configuration Settings

		private static string GetLatestVersion()
		{
			var buildPackagesBranchDirectory = new DirectoryInfo(BuildPackagesBranchPath);
			DirectoryInfo latestVersionFolder = buildPackagesBranchDirectory.GetDirectories().OrderByDescending(f => f.LastWriteTime).First();

			return latestVersionFolder?.Name;
		}
		
		private static string GetTargetHost()
		{
			string environmentVariableName = AppSettingString("JenkinsBuildHostEnvironmentVariableName");

			if (environmentVariableName != null)
			{
				string environmentSetting = Environment.GetEnvironmentVariable(environmentVariableName, EnvironmentVariableTarget.Machine);
			    if (!string.IsNullOrEmpty(environmentSetting))
			    {
			        return environmentSetting;
			    }
			}

			return AppSettingString("targetHost");
		}
		
		private static string GetTargetDbHost()
	    {
	        string environmentVariableName = AppSettingString("JenkinsBuildHostEnvironmentVariableName");

	        if (environmentVariableName != null)
	        {
	            return Environment.GetEnvironmentVariable(environmentVariableName, EnvironmentVariableTarget.Machine);
	        }

	        return AppSettingString("targetDbHost");
	    }

		public static bool UseIpRapFile()
		{
			string environmentVariableName = AppSettingString("JenkinsBuildUseIPRapFile");

			if (environmentVariableName != null)
			{
				string environmentSetting = Environment.GetEnvironmentVariable(environmentVariableName, EnvironmentVariableTarget.Machine);
				if (!string.IsNullOrEmpty(environmentSetting))
				{
					return Convert.ToBoolean(environmentSetting);
				}
			}
			return true;
		}

		public static bool UseLegacyTemplateName()
		{
			string environmentVariableName = AppSettingString("UseLegacyTemplateName");

			if (environmentVariableName != null)
			{
				string environmentSetting = Environment.GetEnvironmentVariable(environmentVariableName, EnvironmentVariableTarget.Machine);
				if (!string.IsNullOrEmpty(environmentSetting))
				{
					return Convert.ToBoolean(environmentSetting);
				}
			}
			return false;
		}
	}
}
