#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
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
	}
}
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
