#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.IntegrationPointsServices
{
	public class WebClientFactory
	{
		private readonly IHelper _helper;
		private readonly IRsapiClientWithWorkspaceFactory _factory;
		private readonly IWorkspaceContext _workspaceIdProvider;

		public WebClientFactory(IHelper helper, IRsapiClientWithWorkspaceFactory factory, IWorkspaceContext workspaceIdProvider)
		{
			_helper = helper;
			_factory = factory;
			_workspaceIdProvider = workspaceIdProvider;
		}

		public IRSAPIClient CreateClient()
		{
			int workspaceId = _workspaceIdProvider.GetWorkspaceID();
			return _factory.CreateUserClient(workspaceId);
		}

		public IDBContext CreateDbContext()
		{
			int workspaceId = _workspaceIdProvider.GetWorkspaceID();
			return _helper.GetDBContext(workspaceId);
		}
	}
}
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
