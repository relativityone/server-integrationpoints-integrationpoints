using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Helpers
{
	public interface IPermissionRepositoryFactory
	{
		IPermissionRepository Create(IHelper helper, int workspaceArtifactId);
	}
}