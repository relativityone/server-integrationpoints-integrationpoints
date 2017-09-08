using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class ServiceContextHelperForKelperService : IServiceContextHelper
	{
		private readonly IServiceHelper _helper;
		private int _workspaceArtifactId;
		private readonly IRsapiClientFactory _rsapiClientFactory;

		public ServiceContextHelperForKelperService(IServiceHelper helper, int workspaceArtifactId, IRsapiClientFactory rsapiClientFactory)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
			_rsapiClientFactory = rsapiClientFactory;
		}

		public int WorkspaceID {
			get { return _workspaceArtifactId; }
			set { _workspaceArtifactId = value; }
		}

		public int GetEddsUserID()
		{
			return _helper.GetAuthenticationManager().UserInfo.ArtifactID;
		}

		public int GetWorkspaceUserID()
		{
			return _helper.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID;
		}

		public IDBContext GetDBContext(int workspaceId = -1)
		{
			return _helper.GetDBContext(workspaceId);
		}

		public IRSAPIService GetRsapiService()
		{
			return ServiceContextFactory.CreateRSAPIService(_helper, WorkspaceID);
		}

		public IRSAPIClient GetRsapiClient()
		{
			IRSAPIClient client = _rsapiClientFactory.CreateUserClient(_workspaceArtifactId);
			return client;
		}
	}
}