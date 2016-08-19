﻿using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using Castle.DynamicProxy.Generators.Emitters;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class SharedVariables
	{
		#region User Settings

		public static string RelativityUserName { get; set; } = ConfigurationManager.AppSettings["relativityUserName"];

		public static string RelativityPassword { get; set; } = ConfigurationManager.AppSettings["relativityPassword"];

		public static string RelativityUserFirstName { get; set; } = ConfigurationManager.AppSettings["relativityUserFirstName"];

		public static string RelativityUserLastName { get; set; } = ConfigurationManager.AppSettings["relativityUserLastName"];

		public static string UserFullName => $"{RelativityUserLastName}, {RelativityUserFirstName}";

		#endregion User Settings

		#region Relativity Settings

		public static string RsapiClientUri => $"http://{TargetHost}/Relativity.Services";

		public static Uri RsapiClientServiceUri => new Uri($"{RsapiClientUri}/");

		public static string RestServer => $"http://{TargetHost}/Relativity.Rest/";

		public static Uri RestClientServiceUri => new Uri($"{RestApi}/api");

		public static string RestApi => $"http://{TargetHost}/Relativity.Rest";

		public static string RelativityWebApiUrl => $"http://{TargetHost}/RelativityWebAPI/";

		#endregion Relativity Settings

		#region ConnectionString Settings

		public static string TargetHost => ConfigurationManager.AppSettings["targetHost"];

		public static string DatabaseUserId { get; set; } = ConfigurationManager.AppSettings["databaseUserId"];

		public static string DatabasePassword { get; set; } = ConfigurationManager.AppSettings["databasePassword"];

		public static string EddsConnectionString => String.Format(ConfigurationManager.AppSettings["connectionStringEDDS"], TargetHost, DatabaseUserId, DatabasePassword);

		public static string WorkspaceConnectionStringFormat => String.Format(ConfigurationManager.AppSettings["connectionStringWorkspace"], "{0}", TargetHost, DatabaseUserId, DatabasePassword);

		#endregion ConnectionString Settings

		#region RAP File Settings

		public static string ApplicationPath { get; set; } = ConfigurationManager.AppSettings["applicationPath"];

		public static string ApplicationRapFileName { get; set; } = ConfigurationManager.AppSettings["applicationRapFileName"];

		public static string BuildPackagesBranchPath => Path.Combine(ConfigurationManager.AppSettings["buildPackages"], ConfigurationManager.AppSettings["branch"]);

		public static string LatestRapLocationFromBuildPackages => Path.Combine(BuildPackagesBranchPath, LatestRapVersionFromBuildPackages);

		public static string LatestRapVersionFromBuildPackages => GetLatestVersion();

		public static string RapFileLocation
		{
			get
			{
				string value = Environment.GetEnvironmentVariable("rapFileLocation", EnvironmentVariableTarget.Machine);
				if (value == null)
				{
					value = Environment.GetEnvironmentVariable("rapFileLocation", EnvironmentVariableTarget.User);
					if (value == null)
					{
						value = @"C:\SourceCode\IntegrationPoints\source\bin\Application\RelativityIntegrationPoints.Auto.rap";
					}
				}
				return value;
			}
		}

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
	}
}