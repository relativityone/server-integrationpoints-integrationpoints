using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public abstract class WorkspaceHelperBase
    {
        public WorkspaceTest Workspace { get; }

        public WorkspaceHelperBase(WorkspaceTest workspace)
        {
            Workspace = workspace;
        }
    }
}