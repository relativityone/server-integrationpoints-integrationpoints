using System;
using System.Configuration;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class SharedVariables
	{
		public static string TargetHost => ConfigurationManager.AppSettings["targetHost"];

		public static string RsapiClientUri => $"http://{TargetHost}/Relativity.Services";
		public static Uri RsapiClientServiceUri => new Uri($"{RsapiClientUri}/api");

		public static string RelativityUserName { get; set; } = ConfigurationManager.AppSettings["userName"];

		public static string RelativityPassword { get; set; } = ConfigurationManager.AppSettings["password"];

		public static string RestServer => $"http://{TargetHost}/Relativity.Rest/";

		public static Uri RestClientServiceUri => new Uri($"{RestApi}/api");

		public static string RestApi => $"http://{TargetHost}/Relativity.Rest";

		public static string RelativityWebApiUrl => $"http://{TargetHost}/RelativityWebAPI/";

		public static string RapFileLocation
		{
			get
			{
				string value = Environment.GetEnvironmentVariable("rapFileLocation");
				if (value == null)
				{
					value = @"C:\SourceCode\IntegrationPoints\source\bin\Application\RelativityIntegrationPoints.Auto.rap";
				}
				return value;
			}
		}

		public static string EddsConnectionString => ConfigurationManager.AppSettings["connectionStringEDDS"];

		public static string WorkspaceConnectionStringFormat => ConfigurationManager.AppSettings["connectionStringWorkspace"];
	}
}