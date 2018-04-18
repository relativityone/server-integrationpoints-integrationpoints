using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class ServiceContextHelperForAgent : IServiceContextHelper
	{
		public ServiceContextHelperForAgent(IAgentHelper helper, int workspaceId)
		{
			this.helper = helper;
			this.WorkspaceID = workspaceId;
		}
		
		private IAgentHelper helper { get; set; }
		public int WorkspaceID { get; set; }
		public int GetEddsUserID() { return helper.GetAuthenticationManager().UserInfo.ArtifactID; }
		public int GetWorkspaceUserID() { return helper.GetAuthenticationManager().UserInfo.WorkspaceUserArtifactID; }
		public IDBContext GetDBContext() { return helper.GetDBContext(this.WorkspaceID); }
		public IRSAPIService GetRsapiService()
		{
			if (this.WorkspaceID > 0)
				return ServiceContextFactory.CreateRSAPIService(helper, WorkspaceID);
			else
				return null;
		}

		public IDBContext GetDBContext(int workspaceID = -1) { return helper.GetDBContext(workspaceID); }
	}
}
