using System;
using System.Configuration;

namespace Relativity.Sync.Tests.System
{
	public static class AppSettings
	{
		public static Uri RelativityUrl => new Uri(ConfigurationManager.AppSettings.Get("RelativityUrl"));
		public static Uri RelativityServicesUrl => new Uri(ConfigurationManager.AppSettings.Get("RelativityServicesUrl"));
		public static Uri RelativityRestUrl => new Uri(ConfigurationManager.AppSettings.Get("RelativityRestUrl"));
		public static string RelativityUserName => ConfigurationManager.AppSettings.Get("RelativityUserName");
		public static string RelativityUserPassword => ConfigurationManager.AppSettings.Get("RelativityUserPassword");
	}
}