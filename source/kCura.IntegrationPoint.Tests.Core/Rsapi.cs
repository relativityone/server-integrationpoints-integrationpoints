using Relativity.API;

using System;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class Rsapi : HelperBase
	{
		public Rsapi(Helper helper) : base(helper)
		{
		}

		public IRSAPIClient CreateRsapiClient()
		{
			Uri relativityServicesUri = new Uri(SharedVariables.RsapiClientUri);
			IRSAPIClient client = new RSAPIClient(relativityServicesUri, new UsernamePasswordCredentials(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword));
			return client;
		}

		public IRSAPIClient CreateRsapiClient(ExecutionIdentity identify)
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
				IRSAPIClient client = new RSAPIClient(relativityServicesUri, new UsernamePasswordCredentials("relativity.admin@kcura.com", "Test1234!"));
				return client;
			}
		}
	}
}