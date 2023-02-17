using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;

namespace Relativity.IntegrationPoints.Services.JobHistory
{
    public class JobHistoryAccess : IJobHistoryAccess
    {
        private readonly IDestinationParser _destinationParser;

        public JobHistoryAccess(IDestinationParser destinationParser)
        {
            _destinationParser = destinationParser;
        }

        public IList<JobHistoryModel> Filter(IList<JobHistoryModel> allJobHistories, IList<int> workspacesWithAccess)
        {
            return allJobHistories.Where(x => DoesUserHavePermissionToThisDestinationWorkspace(workspacesWithAccess, x.DestinationWorkspace)).ToList();
        }

        public IList<JobHistoryModel> Filter(IList<JobHistoryModel> allJobHistories, IDictionary<int, IList<int>> workspacesWithAccess)
        {
            return allJobHistories.Where(x =>
            {
                int instanceId;
                if (x.DestinationInstance == FederatedInstanceManager.LocalInstance.Name)
                {
                    instanceId = -1;
                }
                else
                {
                    instanceId = _destinationParser.GetArtifactId(x.DestinationInstance);
                }
                return DoesUserHavePermissionToThisDestinationWorkspace(workspacesWithAccess[instanceId], x.DestinationWorkspace);
            }).ToList();
        }

        private bool DoesUserHavePermissionToThisDestinationWorkspace(IList<int> accessibleWorkspaces, string destinationWorkspace)
        {
            int workspaceArtifactId = _destinationParser.GetArtifactId(destinationWorkspace);
            return accessibleWorkspaces.Any(t => t == workspaceArtifactId);
        }
    }
}
