using System;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Rsapi
	{
		public static IRSAPIClient CreateRsapiClient()
		{
			IRSAPIClient client = new RSAPIClient(SharedVariables.RsapiUri, new UsernamePasswordCredentials(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword));
			return client;
		}
	}
}