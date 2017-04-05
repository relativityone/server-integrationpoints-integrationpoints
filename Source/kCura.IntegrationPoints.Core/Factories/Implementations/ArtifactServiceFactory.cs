using kCura.IntegrationPoints.Core.Services;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ArtifactServiceFactory : IArtifactServiceFactory
	{
		public IArtifactService CreateArtifactService(IHelper helper, IHelper targetHelper)
		{
			var rsapiClient = targetHelper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
			return new ArtifactService(rsapiClient, helper);
		}
	}
}
