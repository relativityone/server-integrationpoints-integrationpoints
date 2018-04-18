using System;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Rsapi
	{
		public static IRSAPIClient CreateRsapiClient()
		{
			Uri relativityServicesUri = new Uri(SharedVariables.RsapiClientUri);
			IRSAPIClient client = new RSAPIClient(relativityServicesUri, new UsernamePasswordCredentials(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword));
			return client;
		}
	}
}