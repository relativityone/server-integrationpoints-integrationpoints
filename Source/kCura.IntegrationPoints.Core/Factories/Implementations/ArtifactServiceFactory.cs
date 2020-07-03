#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ArtifactServiceFactory : IArtifactServiceFactory
	{
		private readonly IRsapiClientFactory _rsapiClientFactory;

		public ArtifactServiceFactory(IRsapiClientFactory rsapiClientFactory)
		{
			_rsapiClientFactory = rsapiClientFactory;
		}

		public IArtifactService CreateArtifactService(IHelper helper)
		{
			IRSAPIClient rsapiClientWithLogging = _rsapiClientFactory.CreateUserClient(helper);

			return new ArtifactService(rsapiClientWithLogging, helper);
		}
	}
}
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
