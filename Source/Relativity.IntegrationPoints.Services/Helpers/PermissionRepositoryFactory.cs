using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Relativity.API;

namespace Relativity.IntegrationPoints.Services.Helpers
{
    public class PermissionRepositoryFactory : IPermissionRepositoryFactory
    {
        public IPermissionRepository Create(IHelper helper, int workspaceArtifactId)
        {
            return new PermissionRepository(helper, workspaceArtifactId);
        }
    }
}
