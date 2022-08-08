using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace Relativity.IntegrationPoints.Services.Helpers
{
    public interface IPermissionRepositoryFactory
    {
        IPermissionRepository Create(IHelper helper, int workspaceArtifactId);
    }
}