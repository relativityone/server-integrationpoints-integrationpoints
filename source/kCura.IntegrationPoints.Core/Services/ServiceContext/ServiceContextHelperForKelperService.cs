using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class ServiceContextHelperForKelperService : IServiceContextHelper
	{
		private readonly IServiceHelper _helper;
		private int _workspaceArtifactId;

		public ServiceContextHelperForKelperService(IServiceHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
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

		public IRSAPIClient GetRsapiClient(ExecutionIdentity identity)
		{
			IRSAPIClient client = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(identity);
			client.APIOptions.WorkspaceID = _workspaceArtifactId;
			return client;
		}
	}
}