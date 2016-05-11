
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
			Uri relativityServicesUri = new Uri(SharedVariables.RsapiClientUri);
			IRSAPIClient client = new RSAPIClient(relativityServicesUri, new UsernamePasswordCredentials(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword));
			return client;
		}
	}
}
