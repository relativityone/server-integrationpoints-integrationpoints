using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Interfaces.Private.Helpers
{
	public class PermissionRepositoryFactory : IPermissionRepositoryFactory
	{
		public IPermissionRepository Create(IHelper helper, int workspaceArtifactId)
		{
			return new PermissionRepository(helper, workspaceArtifactId);
		}
	}
}