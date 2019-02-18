using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.IntegrationPointsServices
{
	public class WebClientFactory
	{
		private readonly IHelper _helper;
		private readonly IRsapiClientWithWorkspaceFactory _factory;
		private readonly IWorkspaceIdProvider _workspaceIdProvider;

		public WebClientFactory(IHelper helper, IRsapiClientWithWorkspaceFactory factory, IWorkspaceIdProvider workspaceIdProvider)
		{
			_helper = helper;
			_factory = factory;
			_workspaceIdProvider = workspaceIdProvider;
		}

		public IRSAPIClient CreateClient()
		{
			int workspaceId = _workspaceIdProvider.GetWorkspaceId();
			return _factory.CreateUserClient(workspaceId);
		}

		public IDBContext CreateDbContext()
		{
			int workspaceId = _workspaceIdProvider.GetWorkspaceId();
			return _helper.GetDBContext(workspaceId);
		}
	}
}