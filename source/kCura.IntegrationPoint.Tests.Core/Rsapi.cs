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
	}
}