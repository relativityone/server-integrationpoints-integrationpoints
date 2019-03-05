using System.Configuration;

namespace Relativity.Sync.Tests.System
{
	public static class AppSettings
	{
		public static string RelativityHostName => ConfigurationManager.AppSettings.Get("RelativityHostName");
		public static string RelativityUserName => ConfigurationManager.AppSettings.Get("RelativityUserName");
		public static string RelativityUserPassword => ConfigurationManager.AppSettings.Get("RelativityUserPassword");
	}
}