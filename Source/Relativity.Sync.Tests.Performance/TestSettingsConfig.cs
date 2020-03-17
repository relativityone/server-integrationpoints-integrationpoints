using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Performance
{
	internal static class TestSettingsConfig
	{
		public static bool IsSettingsFileSet => TestContext.Parameters.Names.Any();

		public static string ServerBindingType => GetConfigValue(nameof(ServerBindingType));

		public static string RelativityHostAddress => GetConfigValue(nameof(RelativityHostAddress));

		public static Uri RelativityUrl => BuildUri("Relativity/");

		public static Uri RelativityServicesUrl => BuildUri("Relativity.Services/");

		public static Uri RelativityRestUrl => BuildUri("Relativity.Rest/api/");

		public static Uri RelativityWebApiUrl => BuildUri("RelativityWebAPI/");

		public static string RsapiServicesHostAddress => GetConfigValue(nameof(RsapiServicesHostAddress));

		public static string AdminUsername => GetConfigValue(nameof(AdminUsername));

		public static string AdminPassword => GetConfigValue(nameof(AdminPassword));

		public static string ARMRapPath => GetConfigValue(nameof(ARMRapPath));

		public static string ARMTestServicesRapPath => GetConfigValue(nameof(ARMTestServicesRapPath));

		private static Uri BuildUri(string relativeUrl)
			=> new UriBuilder(ServerBindingType, RelativityHostAddress, -1, relativeUrl).Uri;

		private static string GetConfigValue(string name) => TestContext.Parameters.Exists(name)
			? TestContext.Parameters[name]
			: string.Empty;
	}
}
