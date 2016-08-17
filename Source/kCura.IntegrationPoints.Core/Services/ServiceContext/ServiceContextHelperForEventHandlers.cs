using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class ServiceContextHelperForEventHandlers : IServiceContextHelper
	{
		public ServiceContextHelperForEventHandlers(IEHHelper helper, int workspaceId, RsapiClientFactory factory)
		{
			this.helper = helper;
			this.WorkspaceID = workspaceId;
			this.factory = factory;
		}

		private RsapiClientFactory factory { get; set; }
		private IEHHelper helper { get; set; }
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
		public IRSAPIClient GetRsapiClient(ExecutionIdentity identity = ExecutionIdentity.CurrentUser)
		{
			if (this.WorkspaceID > 0)
				return factory.CreateClientForWorkspace(this.WorkspaceID, identity);
			else
				return null;
		}
		public IDBContext GetDBContext(int workspaceID = -1) { return helper.GetDBContext(workspaceID); }
	}
}
