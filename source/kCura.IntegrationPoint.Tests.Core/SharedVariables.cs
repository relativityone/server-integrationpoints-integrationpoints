
namespace kCura.IntegrationPoint.Tests.Core
{
	using System;
	using System.Configuration;

	public static class SharedVariables
	{
		public static int WorkspaceArtifactId => Convert.ToInt32(ConfigurationManager.AppSettings["workspaceArtifactId"]);

		public static string RsapiClientUri => ConfigurationManager.AppSettings["rsapClientUri"];

		public static string RelativityUserName => ConfigurationManager.AppSettings["userName"];

		public static string RelativityPassword => ConfigurationManager.AppSettings["password"];
	}
}