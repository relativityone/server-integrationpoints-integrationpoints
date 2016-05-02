
namespace kCura.IntegrationPoint.Tests.Core
{
	using System;
	using Relativity.Client;

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
