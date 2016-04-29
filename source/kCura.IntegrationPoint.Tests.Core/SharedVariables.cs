﻿
namespace kCura.IntegrationPoint.Tests.Core
{
	using System;
	using System.Configuration;

	public class SharedVariables
	{
		public int WorkspaceArtifactId => Convert.ToInt32(ConfigurationManager.AppSettings["workspaceArtifactId"]);

		public string RsapiClientUri => ConfigurationManager.AppSettings["rsapClientUri"];

		public string RelativityUserName => ConfigurationManager.AppSettings["userName"];

		public string RelativityPassword => ConfigurationManager.AppSettings["password"];
	}
}