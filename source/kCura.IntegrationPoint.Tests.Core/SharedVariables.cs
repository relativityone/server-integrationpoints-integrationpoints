using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using ContentAnalyst.Web.TextCategorizerIndexService;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class SharedVariables
	{
		public static string TargetHost => ConfigurationManager.AppSettings["targetHost"];

		public static int WorkspaceArtifactId => Convert.ToInt32(ConfigurationManager.AppSettings["workspaceArtifactId"]);

		public static string RsapiClientUri => $"http://{TargetHost}/Relativity.Services";
		public static Uri RsapiClientServiceUri => new Uri($"{RsapiClientUri}/api");

		public static string RelativityUserName { get; set; } = ConfigurationManager.AppSettings["userName"];

		public static string RelativityPassword { get; set; } = ConfigurationManager.AppSettings["password"];

		public static string RestServer => $"http://{TargetHost}/Relativity.Rest/";

		public static Uri RestClientServiceUri => new Uri($"{RestApi}/api");

		public static string RestApi => $"http://{TargetHost}/Relativity.Rest";

		public static string RelativityWebApiUrl => $"http://{TargetHost}/RelativityWebAPI/";

		public static string RapFileLocation => ConfigurationManager.AppSettings["rapFileLocation"];

		public static string EddsConnectionString => ConfigurationManager.AppSettings["connectionStringEDDS"];

		public static string WorkspaceConnectionStringFormat => ConfigurationManager.AppSettings["connectionStringWorkspace"];
	}
}