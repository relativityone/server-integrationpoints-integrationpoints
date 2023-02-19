using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public abstract class WorkspaceHelperBase
    {
        protected WorkspaceTest Workspace { get; }

        protected WorkspaceHelperBase(WorkspaceTest workspace)
        {
            Workspace = workspace;
        }
    }
}
