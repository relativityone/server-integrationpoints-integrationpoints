using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class SharedVariables
	{
		#region User Settings

		public static string RelativityUserName { get; set; } = ConfigurationManager.AppSettings["relativityUserName"];

		public static string RelativityPassword { get; set; } = ConfigurationManager.AppSettings["relativityPassword"];

		public static string RelativityUserFirstName { get; set; } = ConfigurationManager.AppSettings["relativityUserFirstName"];

		public static string RelativityUserLastName { get; set; } = ConfigurationManager.AppSettings["relativityUserLastName"];

		public static string UserFullName => $"{RelativityUserLastName}, {RelativityUserFirstName}";

        #endregion User Settings
        
        #region UI Tests Settings
        public static int UiImplicitWaitInSec { get; set; } = int.Parse(ConfigurationManager.AppSettings["ui.implicitWaitInSec"]);
        public static int UiWaitForAjaxCallsInSec { get; set; } = int.Parse(ConfigurationManager.AppSettings["ui.waitForAjaxCallsInSec"]);
        #endregion UI Tests Settings
        
        #region Relativity Settings

		public static string ProtocolVersion => ConfigurationManager.AppSettings["ProtocolVersion"];

		public static string RsapiClientUri => $"{ProtocolVersion}://{TargetHost}/Relativity.Services";

		public static Uri RsapiClientServiceUri => new Uri($"{RsapiClientUri}/");

		public static string RestServer => $"{ProtocolVersion}://{TargetHost}/Relativity.Rest/";

		public static Uri RestClientServiceUri => new Uri($"{RestServer}/api");

		public static string RelativityWebApiUrl => $"{ProtocolVersion}://{TargetHost}/RelativityWebAPI/";

		#endregion Relativity Settings

		#region ConnectionString Settings

		public static string TargetHost => GetTargetHost();

		public static string DatabaseUserId { get; set; } = ConfigurationManager.AppSettings["databaseUserId"];

		public static string DatabasePassword { get; set; } = ConfigurationManager.AppSettings["databasePassword"];

		public static string EddsConnectionString => String.Format(ConfigurationManager.AppSettings["connectionStringEDDS"], TargetHost, DatabaseUserId, DatabasePassword);

		public static string WorkspaceConnectionStringFormat => String.Format(ConfigurationManager.AppSettings["connectionStringWorkspace"], "{0}", TargetHost, DatabaseUserId, DatabasePassword);

		public static int KeplerTimeout => int.Parse(ConfigurationManager.AppSettings["keplerTimeout"]);

		#endregion ConnectionString Settings

		#region RAP File Settings

		public static string ApplicationPath { get; set; } = ConfigurationManager.AppSettings["applicationPath"];

		public static string ApplicationRapFileName { get; set; } = ConfigurationManager.AppSettings["applicationRapFileName"];

		public static string BuildPackagesBranchPath => Path.Combine(ConfigurationManager.AppSettings["buildPackages"], ConfigurationManager.AppSettings["branch"]);

		public static string LatestRapLocationFromBuildPackages => Path.Combine(BuildPackagesBranchPath, LatestRapVersionFromBuildPackages);

		public static string LatestRapVersionFromBuildPackages => GetLatestVersion();

		public static bool UseLocalRap => bool.Parse(ConfigurationManager.AppSettings["UseLocalRAP"]);

		public static string RapFileLocation => ConfigurationManager.AppSettings["LocalRAPFileLocation"];

		#endregion RAP File Settings

		#region LDAP Configuration Settings

		public static string LdapConnectionPath { get; set; } = ConfigurationManager.AppSettings["ldapConnectionPath"];

		public static string LdapUsername { get; set; } = ConfigurationManager.AppSettings["ldapUsername"];

		public static string LdapPassword { get; set; } = ConfigurationManager.AppSettings["ldapPassword"];

		#endregion LDAP Configuration Settings

		private static string GetLatestVersion()
		{
			DirectoryInfo buildPackagesBranchDirectory = new DirectoryInfo(BuildPackagesBranchPath);
			DirectoryInfo latestVersionFolder = buildPackagesBranchDirectory.GetDirectories().OrderByDescending(f => f.LastWriteTime).First();

			return latestVersionFolder?.Name;
		}
		
		private static string GetTargetHost()
		{
			string environmentVariableName = ConfigurationManager.AppSettings["JenkinsBuildHostEnvironmentVariableName"];

			if (environmentVariableName != null)
			{
				return Environment.GetEnvironmentVariable(environmentVariableName, EnvironmentVariableTarget.Machine);
			}

			return ConfigurationManager.AppSettings["targetHost"];
		}
	}
}