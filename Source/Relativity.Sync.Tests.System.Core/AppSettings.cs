﻿using System;
using System.Configuration;
using System.Linq;
using NUnit.Framework;
using System.Text;

namespace Relativity.Sync.Tests.System.Core
{
	public static class AppSettings
	{
		private static Uri _relativityRestUrl;
		private static Uri _relativityServicesUrl;
		private static Uri _relativityWebApiUrl;
		private static Uri _relativityUrl;

		public static bool IsSettingsFileSet => TestContext.Parameters.Names.Any();

		public static string RelativityHostName => TestContext.Parameters.Exists("RelativityHostAddress")
			? TestContext.Parameters["RelativityHostAddress"]
			: ConfigurationManager.AppSettings.Get(nameof(RelativityHostName));

		public static string RelativityServicesAddress => GetAppSettingOrDefaultValue("RsapiServicesHostAddress");

		public static Uri RelativityUrl => _relativityUrl ?? (_relativityUrl = BuildHostNamedBasedUri("Relativity"));

		public static Uri RsapiServicesUrl => _relativityServicesUrl ?? (_relativityServicesUrl = BuildServicesBasedUri("Relativity.Services"));

		public static Uri RelativityRestUrl => _relativityRestUrl ?? (_relativityRestUrl = BuildHostNamedBasedUri("Relativity.Rest/api"));

		public static Uri RelativityWebApiUrl => _relativityWebApiUrl ?? (_relativityWebApiUrl = BuildHostNamedBasedUri("RelativityWebAPI"));

		public static string RelativityUserName => GetConfigValue("AdminUsername");

		public static string RelativityUserPassword => GetConfigValue("AdminPassword");

		public static string BasicAccessToken => Convert.ToBase64String(Encoding.ASCII.GetBytes($"{RelativityUserName}:{RelativityUserPassword}"));

		public static string SqlServer => GetConfigValue("SqlServer");

		public static string SqlUsername => GetConfigValue("SqlUsername");

		public static string SqlPassword => GetConfigValue("SqlPassword");

		public static string ConnectionStringEDDS => string.Format("Data Source={0};Initial Catalog=EDDS", SqlServer);

		public static string ConnectionStringWorkspace(int workspaceID) => string.Format("Data Source={0};Initial Catalog=EDDS{1}", SqlServer, workspaceID);

		public static string AzureStorageAccount => GetConfigValue(nameof(AzureStorageAccount));

		public static string AzureStorageAuthorizationKey => GetConfigValue(nameof(AzureStorageAuthorizationKey));

		public static string AzureStorageConnectionString
			=> string.Format(@"DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix=core.windows.net", AzureStorageAccount, AzureStorageAuthorizationKey);

		public static string AzureStoragePerformanceContainer => GetConfigValue(nameof(AzureStoragePerformanceContainer));

		public static bool SuppressCertificateCheck => bool.Parse(GetConfigValue("SuppressCertificateCheck"));

		public static int ResourcePoolId => int.Parse(GetConfigValue("ResourcePoolId"));

		public static string RelativeArchivesLocation => GetConfigValue("RelativeArchivesLocation");

		public static string RemoteArchivesLocation => GetConfigValue("RemoteArchivesLocation");

		public static string PerformanceResultsFilePath => GetConfigValue("PerformanceResultsFilePath");

		public static bool UseLogger => !bool.TryParse(GetConfigValue("SuppressCertificateCheck"), out bool useLogger) || useLogger;

		private static Uri BuildHostNamedBasedUri(string path)
		{
			if (string.IsNullOrEmpty(RelativityHostName))
			{
				throw new ConfigurationErrorsException($"{nameof(RelativityHostName)} is not set. Please supply it's value in app.config in order to run system tests.");
			}

			var uriBuilder = new UriBuilder("https", RelativityHostName, -1, path);
			return uriBuilder.Uri;
		}

		private static Uri BuildServicesBasedUri(string path)
		{
			var uriBuilder = new UriBuilder("https", RelativityServicesAddress ?? RelativityHostName, -1, path);
			return uriBuilder.Uri;
		}
		private static string GetConfigValue(string name) => TestContext.Parameters.Exists(name)
			? TestContext.Parameters[name]
			: ConfigurationManager.AppSettings.Get(name);

		private static string GetAppSettingOrDefaultValue(string name) => string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get(name))
			? TestContext.Parameters[name]
			: RelativityHostName;
	}
}