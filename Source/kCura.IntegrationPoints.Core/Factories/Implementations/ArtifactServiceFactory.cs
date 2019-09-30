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
