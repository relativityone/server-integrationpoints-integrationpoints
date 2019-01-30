using kCura.Relativity.Client;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Rsapi
	{
		public static IRSAPIClient CreateRsapiClient()
		{
			return new RSAPIClient(
				SharedVariables.RsapiUri,
				new UsernamePasswordCredentials(
					SharedVariables.RelativityUserName,
					SharedVariables.RelativityPassword
				)
			);
		}
	}
}