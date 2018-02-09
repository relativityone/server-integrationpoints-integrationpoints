using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.RSAPIClient
{
	public interface IRsapiClientFactory
	{
		IRSAPIClient CreateClient(IHelper helper, ExecutionIdentity identity);
		IRSAPIClient CreateUserClient(IHelper helper);
		IRSAPIClient CreateAdminClient(IHelper helper);

		IRSAPIClient CreateClient(IServicesMgr serviceManager, IAPILog logger, ExecutionIdentity identity);
		IRSAPIClient CreateUserClient(IServicesMgr servicesMgr, IAPILog logger);
		IRSAPIClient CreateAdminClient(IServicesMgr servicesMgr, IAPILog logger);
		
	}
}
