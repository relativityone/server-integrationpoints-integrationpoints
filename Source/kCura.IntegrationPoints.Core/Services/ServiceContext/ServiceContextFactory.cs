using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class ServiceContextFactory
	{
		public static ICaseServiceContext CreateCaseServiceContext(IEHHelper helper, int workspaceID)
		{
			return new CaseServiceContext(new ServiceContextHelperForEventHandlers(helper, workspaceID, new RsapiClientWithWorkspaceFactory(helper)));
		}

		public static IEddsServiceContext CreateEddsServiceContext(IEHHelper helper)
		{
			return new EddsServiceContext(new ServiceContextHelperForEventHandlers(helper, -1, new RsapiClientWithWorkspaceFactory(helper)));
		}

		public static IRSAPIService CreateRSAPIService(IHelper helper, int workspaceArtifactId)
		{
			return new RSAPIService(helper, workspaceArtifactId);
		}
	}
}
