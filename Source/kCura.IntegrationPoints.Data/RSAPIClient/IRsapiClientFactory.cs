#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.RSAPIClient
{
	public interface IRsapiClientFactory
	{
		IRSAPIClient CreateClient(IHelper helper, ExecutionIdentity identity);
	}
}
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
