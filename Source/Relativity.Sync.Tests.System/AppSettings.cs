using System;
using System.Configuration;

namespace Relativity.Sync.Tests.System
{
	public static class AppSettings
	{
		private static Uri _relativityRestUrl;
		private static Uri _relativityServicesUrl;
		private static Uri _relativityUrl;
		private static readonly UriBuilder Builder = new UriBuilder("https", RelativityHostName);
		private static string RelativityHostName => ConfigurationManager.AppSettings.Get("RelativityHostName");

		public static Uri RelativityUrl => _relativityUrl ?? (_relativityUrl  = BuildUri("Relativity"));
		public static Uri RelativityServicesUrl => _relativityServicesUrl ?? (_relativityServicesUrl = BuildUri(ConfigurationManager.AppSettings.Get("RelativityServicesUrl")));
		public static Uri RelativityRestUrl => _relativityRestUrl ?? (_relativityRestUrl = BuildUri(ConfigurationManager.AppSettings.Get("RelativityRestUrl")));
		public static string RelativityUserName => ConfigurationManager.AppSettings.Get("RelativityUserName");
		public static string RelativityUserPassword => ConfigurationManager.AppSettings.Get("RelativityUserPassword");

		private static Uri BuildUri(string path)
		{
			Builder.Path = path;
			return Builder.Uri;
		}
	}
}