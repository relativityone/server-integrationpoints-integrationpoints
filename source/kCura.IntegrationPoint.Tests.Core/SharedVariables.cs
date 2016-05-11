namespace kCura.IntegrationPoint.Tests.Core
{
	using System;
	using System.Configuration;

	public static class SharedVariables
	{
		private static string TargetHost => ConfigurationManager.AppSettings["targetHost"];

		public static int WorkspaceArtifactId => Convert.ToInt32(ConfigurationManager.AppSettings["workspaceArtifactId"]);

		public static string RsapiClientUri => $"http://{TargetHost}/Relativity.Services";

		public static string RelativityUserName => ConfigurationManager.AppSettings["userName"];

		public static string RelativityPassword => ConfigurationManager.AppSettings["password"];

		public static string RestServer => $"http://{TargetHost}/Relativity.Rest/";

		public static string RelativityWebApiUrl => $"http://{TargetHost}/RelativityWebAPI/";
	}
}