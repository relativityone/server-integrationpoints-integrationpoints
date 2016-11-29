using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using User = kCura.Relativity.Client.DTOs.User;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class WorkspaceManager : IWorkspaceManager
	{
		private readonly IServiceHelper _helper;

		public WorkspaceManager(IServiceHelper helper)
		{
			_helper = helper;
		}

		public IList<int> GetIdsOfWorkspacesUserHasPermissionToView()
		{
			IAuthenticationMgr authenticationManager = _helper.GetAuthenticationManager();
			var userArtifactId = authenticationManager.UserInfo.ArtifactID;

			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
			{
				User user = rsapiClient.Repositories.User.ReadSingle(userArtifactId);
				FieldValueList<Workspace> workspaces = user.Workspaces;
				return workspaces.Select(x => x.ArtifactID).ToList();
			}
		}
	}
}