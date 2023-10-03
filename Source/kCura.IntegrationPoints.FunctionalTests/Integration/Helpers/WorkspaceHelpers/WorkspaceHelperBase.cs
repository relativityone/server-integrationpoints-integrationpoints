using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public abstract class WorkspaceHelperBase
    {
        protected WorkspaceFake Workspace { get; }

        protected WorkspaceHelperBase(WorkspaceFake workspace)
        {
            Workspace = workspace;
        }
    }
}
