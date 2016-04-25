using System;
using System.Configuration;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoint.Tests.Core
{
	public abstract class IntegrationTestBase
	{
		protected int WorkspaceArtifactId => Convert.ToInt32(ConfigurationManager.AppSettings["workspaceArtifactId"]);

		protected string RsapiClientUri => ConfigurationManager.AppSettings["rsapClientUri"];

		protected string RelativityUserName => ConfigurationManager.AppSettings["userName"];

		protected string RelativityPassword => ConfigurationManager.AppSettings["password"];

		protected IRSAPIClient RsapiClient
		{
			get
			{
				var client = new RSAPIClient(new Uri(RsapiClientUri),
							 new UsernamePasswordCredentials(RelativityUserName, RelativityPassword))
				{
					APIOptions = { WorkspaceID = WorkspaceArtifactId }
				};
				return client;
			}
		}
	}
}