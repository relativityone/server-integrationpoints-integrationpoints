using System;
using System.Configuration;

namespace Relativity.Sync.Tests.System
{
	public static class AppSettings
	{
		private static Uri _relativityRestUrl;
		private static Uri _relativityServicesUrl;
		private static Uri _relativityWebApiUrl;
		private static Uri _relativityUrl;

		private static string RelativityHostName => ConfigurationManager.AppSettings.Get(nameof(RelativityHostName));

		public static Uri RelativityUrl => _relativityUrl ?? (_relativityUrl = BuildUri("Relativity"));
		public static Uri RelativityServicesUrl => _relativityServicesUrl ?? (_relativityServicesUrl = BuildUri(ConfigurationManager.AppSettings.Get(nameof(RelativityServicesUrl))));
		public static Uri RelativityRestUrl => _relativityRestUrl ?? (_relativityRestUrl = BuildUri(ConfigurationManager.AppSettings.Get(nameof(RelativityRestUrl))));
		public static Uri RelativityWebApiUrl => _relativityWebApiUrl ?? (_relativityWebApiUrl = BuildUri(ConfigurationManager.AppSettings.Get(nameof(RelativityWebApiUrl))));
		public static string RelativityUserName => ConfigurationManager.AppSettings.Get(nameof(RelativityUserName));
		public static string RelativityUserPassword => ConfigurationManager.AppSettings.Get(nameof(RelativityUserPassword));
		public static bool SuppressCertificateCheck => bool.Parse(ConfigurationManager.AppSettings.Get(nameof(SuppressCertificateCheck)));

		private static Uri BuildUri(string path)
		{
			if (string.IsNullOrEmpty(RelativityHostName))
			{
				throw new ConfigurationErrorsException($"{nameof(RelativityHostName)} is not set. Please supply it's value in app.config in order to run system tests.");
			}

			var uriBuilder = new UriBuilder("https", RelativityHostName, -1, path);
			return uriBuilder.Uri;
		}
	}
}