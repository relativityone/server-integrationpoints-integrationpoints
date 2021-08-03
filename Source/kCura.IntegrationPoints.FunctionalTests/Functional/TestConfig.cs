using NUnit.Framework;
using System.Configuration;

namespace Relativity.IntegrationPoints.Tests.Functional
{
	public static class TestConfig
	{
		public static bool DocumentImportEnforceWebMode => bool.Parse(GetConfigValue("DocumentImportEnforceWebMode"));

		public static int DocumentImportTimeout => int.Parse(GetConfigValue("DocumentImportTimeout"));

		private static string GetConfigValue(string name) => TestContext.Parameters.Exists(name)
			? TestContext.Parameters[name]
			: ConfigurationManager.AppSettings.Get(name);
	}
}
