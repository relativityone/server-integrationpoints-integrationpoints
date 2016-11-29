using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class JobHistoryAccess : IJobHistoryAccess
	{
		private readonly IDestinationWorkspaceParser _destinationWorkspaceParser;

		public JobHistoryAccess(IDestinationWorkspaceParser destinationWorkspaceParser)
		{
			_destinationWorkspaceParser = destinationWorkspaceParser;
		}

		public IList<Data.JobHistory> Filter(IEnumerable<Data.JobHistory> allJobHistories, IList<int> workspacesWithAccess)
		{
			return allJobHistories.Where(x => DoesUserHavePermissionToThisDestinationWorkspace(workspacesWithAccess, x.DestinationWorkspace)).ToList();
		}

		private bool DoesUserHavePermissionToThisDestinationWorkspace(IList<int> accessibleWorkspaces, string destinationWorkspace)
		{
			int workspaceArtifactId = _destinationWorkspaceParser.GetWorkspaceArtifactId(destinationWorkspace);
			return accessibleWorkspaces.Any(t => t == workspaceArtifactId);
		}
	}
}