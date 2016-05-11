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
			Uri relativityServicesUri = new Uri(Helper.SharedVariables.RsapiClientUri);
			IRSAPIClient client = new RSAPIClient(relativityServicesUri, new UsernamePasswordCredentials(Helper.SharedVariables.RelativityUserName, Helper.SharedVariables.RelativityPassword));
			return client;
		}
	}
}
