using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
    public class RelativityObjectManagerServiceAdminAccess : RelativityObjectManagerService
    {
        public RelativityObjectManagerServiceAdminAccess(IHelper helper, int workspaceArtifactId) : base(helper, workspaceArtifactId)
        {
            ExecutionIdentity = ExecutionIdentity.System;
        }
    }
}