using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class TestServiceContextHelper : IServiceContextHelper
	{
		private readonly IHelper _helper;
		private readonly IRsapiClientFactory _rsapiClientFactory;

		public TestServiceContextHelper(IHelper helper, int workspaceArtifactId, IRsapiClientFactory rsapiClientFactory)
		{
			_helper = helper;
			_rsapiClientFactory = rsapiClientFactory;
			WorkspaceID = workspaceArtifactId;
		}

		public int WorkspaceID { get; set; }

		public int GetEddsUserID()
		{
			return 9;
		}

		public int GetWorkspaceUserID()
		{
			return 9;
		}

		public IDBContext GetDBContext(int workspaceID = -1)
		{
			return _helper.GetDBContext(workspaceID);
		}

		public IRSAPIService GetRsapiService()
		{
			return ServiceContextFactory.CreateRSAPIService(_helper, WorkspaceID);
		}

		public IRSAPIClient GetRsapiClient()
		{
			return _rsapiClientFactory.CreateUserClient(WorkspaceID);
		}
	}
}