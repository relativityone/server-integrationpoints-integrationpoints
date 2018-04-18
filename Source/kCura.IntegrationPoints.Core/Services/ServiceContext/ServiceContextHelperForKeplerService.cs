using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class ServiceContextHelperForKeplerService : IServiceContextHelper
	{
		private readonly IServiceHelper _helper;
		private int _workspaceArtifactId;

		public ServiceContextHelperForKeplerService(IServiceHelper helper, int workspaceArtifactId)
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
	}
}