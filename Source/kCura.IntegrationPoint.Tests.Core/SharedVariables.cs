using System;
using System.Configuration;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class SharedVariables
	{
		private static readonly Dictionary<string, string> ConfigurationOverrides = new Dictionary<string, string>();

		public static Configuration CustomConfig { get; set; }
		
		static SharedVariables()
		{
			PrepareJeevesConfig();
			ReadConfigKeysWhichWillOverrideExistingValues();
		}

		private static void ReadConfigKeysWhichWillOverrideExistingValues()
		{
			try
			{
				string customConfigurationPath = AppSettingString("CustomizedConfigurationOverridesLocation");
				if (!File.Exists(customConfigurationPath))
				{
					return;
				}


				foreach (var configurationLine in File.ReadAllLines(customConfigurationPath))
				{
					ConfigurationOverrides.Add(configurationLine.Split('|')[0], configurationLine.Split('|')[1]);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(@"Error occured while checking overridable config values: " + e);
			}
		}

		private static void PrepareJeevesConfig()
		{ 
			const string configFileName = "app.jeeves-ci.config";
			try
			{
				string assemblyDir = Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().CodeBase);
				string jeevesConfigPath = Path.Combine(assemblyDir, configFileName);
				if (File.Exists(jeevesConfigPath.Replace(@"file:\", "")))
				{
					Console.WriteLine(@"Jeeves config found: " + jeevesConfigPath);
					MergeConfigurationWithAppConfig(configFileName);
				}
				else
				{
					Console.WriteLine(@"Jeeves config not found: " + jeevesConfigPath);
				}
			}
			catch (ArgumentException ex)
			{
				Console.WriteLine(@"Error occured while checking jeeves file path: " + ex);
			}
			finally
			{
				Console.WriteLine(DumpToString());
			}
		}

		public static string DumpToString()
		{
			ISet<string> allKeys = new SortedSet<string>(ConfigurationManager.AppSettings.AllKeys);
			if (CustomConfig != null)
			{
				allKeys.UnionWith(CustomConfig.AppSettings.Settings.AllKeys);
			}
			var dump = new StringBuilder("SharedVariables:\n");
			foreach (string key in allKeys)
			{
				dump.Append(key).Append(" => ").Append(AppSettingString(key)).Append("\n");
			}
			return dump.ToString();
		}
		
		public static string AppSettingString(string name)
		{
			string overridenValue;
			if (ConfigurationOverrides.TryGetValue(name, out overridenValue))
			{
				return overridenValue;
			}

			return CustomConfig?.AppSettings.Settings[name]?.Value ?? ConfigurationManager.AppSettings[name];
		}

		public static int AppSettingInt(string name)
		{
			return int.Parse(AppSettingString(name));
		}

		public static double AppSettingDouble(string name)
		{
			return double.Parse(AppSettingString(name));
		}

		public static bool AppSettingBool(string name)
		{
			return AppSettingString(name) != null && bool.Parse(AppSettingString(name));
		}

		/// <summary>
		/// Merges settings from custom config file with settings from app.config.
		/// Custom settings take precedence before those defined in app.config.
		/// </summary>
		/// <param name="configFilePath">Path to config file, automatically prefixed by AppDomain.CurrentDomain.BaseDirectory (e.g. ./bin/x64) of given project.</param>
		public static void MergeConfigurationWithAppConfig(string configFilePath)
		{
			string configFileFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFilePath);
			var configFileInfo = new FileInfo(configFileFullPath);
			Assert.IsTrue(configFileInfo.Exists, "Specified config file '{0}' does not exist, full path: '{1}'.",
				configFilePath, configFileInfo.FullName);

			var map = new ExeConfigurationFileMap
			{
				ExeConfigFilename = configFileFullPath
			};

			Configuration c = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
			CustomConfig = c;

			Console.WriteLine(@"Configuration merged with " + configFileFullPath);
		}

		#region User Settings

		public static string RelativityUserName => AppSettingString("AdminUsername");

		public static string RelativityPassword => AppSettingString("AdminPassword");

		public static string RelativityUserFirstName => AppSettingString("relativityUserFirstName");

		public static string RelativityUserLastName => AppSettingString("relativityUserLastName");

		public static string UserFullName => $"{RelativityUserLastName}, {RelativityUserFirstName}";

		#endregion User Settings

		#region UI Tests Settings

		public static int UiImplicitWaitInSec => AppSettingInt("ui.implicitWaitInSec");

		public static int UiWaitForAjaxCallsInSec => AppSettingInt("ui.waitForAjaxCallsInSec");

		public static int UiWaitForPageInSec => AppSettingInt("ui.waitForPageInSec");

		public static string UiUseThisExistingWorkspace => AppSettingString("UI.UseThisExistingWorkspace");

		public static bool UiSkipDocumentImport => AppSettingBool("UI.SkipDocumentImport");

		public static string UiTemplateWorkspace => AppSettingString("UI.TemplateWorkspace");

		public static bool UiUseTapiForFileCopy => AppSettingBool("UI.UseTapiForFileCopy");

		public static double UiTimeoutMultiplier => AppSettingDouble("UI.TimeoutMultiplier");

		public static bool UiSkipUserCreation => AppSettingBool("UI.SkipUserCreation");

		public static string UiBrowser => AppSettingString("UI.Browser");

		public static int UiBrowserWidth => AppSettingInt("UI.BrowserWidth");

		public static int UiBrowserHeight => AppSettingInt("UI.BrowserHeight");

		public static bool UiDriverServiceHideCommandPromptWindow => AppSettingBool("UI.DriverService.HideCommandPromptWindow");

		public static string UiMaxChromeSupportedVersion => AppSettingString("UI.ChromeDriver.MaxSupportedVersion");

		public static string UiDriverServiceLogPath => AppSettingString("UI.DriverService.LogPath");

		public static bool UiOptionsAcceptInsecureCertificates => AppSettingBool("UI.Options.AcceptInsecureCertificates");

		public static bool UiOptionsArgumentsDisableInfobars => AppSettingBool("UI.Options.Arguments.DisableInfoBars");

		public static bool UiOptionsArgumentsHeadless => AppSettingBool("UI.Options.Arguments.Headless");

		public static bool UiOptionsArgumentsIgnoreCertificateErrors => AppSettingBool("UI.Options.Arguments.IgnoreCertificateErrors");

		public static bool UiOptionsAdditionalCapabilitiesAcceptSslCertificates =>
			AppSettingBool("UI.Options.AdditionalCapabilities.AcceptSslCertificates");

		public static bool UiOptionsAdditionalCapabilitiesAcceptInsecureCertificates =>
			AppSettingBool("UI.Options.AdditionalCapabilities.AcceptInsecureCertificates");

		public static bool UiOptionsProfilePreferenceCredentialsEnableService =>
			AppSettingBool("UI.Options.ProfilePreference.CredentialsEnableService");

		public static bool UiOptionsProfilePreferenceProfilePasswordManagerEnabled =>
			AppSettingBool("UI.Options.ProfilePreference.ProfilePasswordManagerEnabled");

		#endregion UI Tests Settings

		#region Relativity Settings

		public static string ProtocolVersion {
			get
			{
				if (AppSettingString("IsHttps") != null)
				{
					return AppSettingBool("IsHttps") ? "https" : "http";
				}

				return ServerBindingType;
			}
		}

		public static string ServerBindingType => AppSettingString("ServerBindingType");

		public static string RSAPIServerAddress => !string.IsNullOrEmpty(AppSettingString("RSAPIServerAddress")) ? AppSettingString("RSAPIServerAddress") : TargetHost;

		public static string RsapiClientUri => $"{ServerBindingType}://{RSAPIServerAddress}/Relativity.Services";

		public static Uri RsapiClientServiceUri => new Uri($"{RsapiClientUri}/");

		public static string RestServer => $"{ServerBindingType}://{TargetHost}/Relativity.Rest/";

		public static Uri RestClientServiceUri => new Uri($"{RestServer}/api");

		public static string RelativityWebApiUrl => $"{ServerBindingType}://{TargetHost}/RelativityWebAPI/";

		#endregion Relativity Settings

		#region ConnectionString Settings

		public static string TargetHost => AppSettingString("RelativityInstanceAddress");
		
		public static string RelativityWebAddress => AppSettingString("RelativityWebAddress");

		public static string TargetDbHost => GetTargetDbHost();
		public static string SqlServerAddress => AppSettingString("SQLServerAddress");

		public static string DatabaseUserId => AppSettingString("SQLUsername");

		public static string DatabasePassword => AppSettingString("SQLPassword");

		public static string EddsConnectionString => string.Format(AppSettingString("connectionStringEDDS"), SqlServerAddress, DatabaseUserId, DatabasePassword);

		public static string WorkspaceConnectionStringFormat => string.Format(AppSettingString("connectionStringWorkspace"), "{0}", SqlServerAddress, DatabaseUserId, DatabasePassword);

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

		#region FTP Configuration Settings

		public static string FTPConnectionPath => AppSettingString("ftpConnectionPath");

		public static string FTPUsername => AppSettingString("ftpUsername");

		public static string FTPPassword => AppSettingString("ftpPassword");

		#endregion

		#region LDAP Configuration Settings

		public static string LdapConnectionPath => AppSettingString("ldapConnectionPath");

		public static string LdapUsername => AppSettingString("ldapUsername");

		public static string LdapPassword => AppSettingString("ldapPassword");

		#endregion LDAP Configuration Settings

		#region Fileshare Configuration Settings

		public static string FileshareLocation => AppSettingString("fileshareLocation");

		#endregion

		private static string GetLatestVersion()
		{
			var buildPackagesBranchDirectory = new DirectoryInfo(BuildPackagesBranchPath);
			DirectoryInfo latestVersionFolder = buildPackagesBranchDirectory.GetDirectories().OrderByDescending(f => f.LastWriteTime).First();

			return latestVersionFolder?.Name;
		}
		
		private static string GetTargetDbHost()
		{
			return !string.IsNullOrEmpty(AppSettingString("targetDbHost")) ? AppSettingString("targetDbHost") : TargetHost;
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
