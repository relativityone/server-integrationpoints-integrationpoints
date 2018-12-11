using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class ServiceContextFactory
	{
		public static ICaseServiceContext CreateCaseServiceContext(IEHHelper helper, int workspaceID)
		{
			return new CaseServiceContext(new ServiceContextHelperForEventHandlers(helper, workspaceID));
		}

		public static IEddsServiceContext CreateEddsServiceContext(IEHHelper helper)
		{
			return new EddsServiceContext(new ServiceContextHelperForEventHandlers(helper, -1));
		}

		public static IRSAPIService CreateRSAPIService(IHelper helper, int workspaceArtifactId)
		{
			var rsapiServiceFactory = new RSAPIServiceFactory(helper);
			return rsapiServiceFactory.Create(workspaceArtifactId);
		}
	}
}
