using System;
using kCura.Relativity.Client;
using Relativity.API;

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

		public static IRSAPIClient CreateRsapiClient(ExecutionIdentity identify)
		{
			if (identify == ExecutionIdentity.CurrentUser)
			{
				Uri relativityServicesUri = new Uri(SharedVariables.RsapiClientUri);
				IRSAPIClient client = new RSAPIClient(relativityServicesUri, new UsernamePasswordCredentials(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword));
				return client;
			}
			else
			{
				Uri relativityServicesUri = new Uri(SharedVariables.RsapiClientUri);
				IRSAPIClient client = new RSAPIClient(relativityServicesUri, new UsernamePasswordCredentials(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword));
				return client;
			}
		}
	}
}