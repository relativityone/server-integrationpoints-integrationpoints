using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.JobHistory;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class JobHistoryAccess : IJobHistoryAccess
	{
		private readonly IDestinationWorkspaceParser _destinationWorkspaceParser;

		public JobHistoryAccess(IDestinationWorkspaceParser destinationWorkspaceParser)
		{
			_destinationWorkspaceParser = destinationWorkspaceParser;
		}

		public IList<JobHistoryModel> Filter(IList<JobHistoryModel> allJobHistories, IList<int> workspacesWithAccess)
		{
			return allJobHistories.Where(x => DoesUserHavePermissionToThisDestinationWorkspace(workspacesWithAccess, x.DestinationWorkspace)).ToList();
		}

		public IList<JobHistoryModel> Filter(IList<JobHistoryModel> allJobHistories, IDictionary<string, IList<int>> workspacesWithAccess)
		{
			return allJobHistories.Where(x =>
			{
				var instanceName = _destinationWorkspaceParser.GetInstanceName(x.DestinationWorkspace);
				return DoesUserHavePermissionToThisDestinationWorkspace(workspacesWithAccess[instanceName], x.DestinationWorkspace);
			}).ToList();
		}

		private bool DoesUserHavePermissionToThisDestinationWorkspace(IList<int> accessibleWorkspaces, string destinationWorkspace)
		{
			int workspaceArtifactId = _destinationWorkspaceParser.GetWorkspaceArtifactId(destinationWorkspace);
			return accessibleWorkspaces.Any(t => t == workspaceArtifactId);
		}
	}
}