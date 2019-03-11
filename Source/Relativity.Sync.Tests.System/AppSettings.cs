using System;
using System.Configuration;

namespace Relativity.Sync.Tests.System
{
	public static class AppSettings
	{
		private static string RelativityHostName => ConfigurationManager.AppSettings.Get("RelativityHostName");

		public static Uri RelativityUrl => new Uri($"https://{RelativityHostName}/Relativity");
		public static Uri RelativityServicesUrl => new Uri($"https://{RelativityHostName}/{ConfigurationManager.AppSettings.Get("RelativityServicesUrl")}");
		public static Uri RelativityRestUrl => new Uri($"https://{RelativityHostName}/{ConfigurationManager.AppSettings.Get("RelativityRestUrl")}");
		public static string RelativityUserName => ConfigurationManager.AppSettings.Get("RelativityUserName");
		public static string RelativityUserPassword => ConfigurationManager.AppSettings.Get("RelativityUserPassword");
	}
}