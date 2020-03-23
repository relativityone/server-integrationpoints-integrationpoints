using NUnit.Framework;
using System;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Relativity.Sync.Tests.System
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

		public static Uri RelativityUrl => _relativityUrl ?? (_relativityUrl = BuildUri("Relativity"));

		public static Uri RelativityServicesUrl => _relativityServicesUrl ?? (_relativityServicesUrl = BuildUri("/Relativity.Services"));

		public static Uri RelativityRestUrl => _relativityRestUrl ?? (_relativityRestUrl = BuildUri("/Relativity.Rest/api"));

		public static Uri RelativityWebApiUrl => _relativityWebApiUrl ?? (_relativityWebApiUrl = BuildUri("/RelativityWebAPI"));

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

		private static Uri BuildUri(string path)
		{
			if (string.IsNullOrEmpty(RelativityHostName))
			{
				throw new ConfigurationErrorsException($"{nameof(RelativityHostName)} is not set. Please supply it's value in app.config in order to run system tests.");
			}

			var uriBuilder = new UriBuilder("https", RelativityHostName, -1, path);
			return uriBuilder.Uri;
		}

		private static string GetConfigValue(string name) => TestContext.Parameters.Exists(name)
			? TestContext.Parameters[name]
			: ConfigurationManager.AppSettings.Get(name);
	}
}