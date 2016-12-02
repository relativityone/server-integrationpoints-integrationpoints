using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface IWorkspaceManager
	{
		IList<int> GetIdsOfWorkspacesUserHasPermissionToView();
	}
}