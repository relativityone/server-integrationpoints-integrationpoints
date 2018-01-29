using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data.Factories
{
	public interface IRelativityObjectManagerFactory
	{
		IRelativityObjectManager CreateRelativityObjectManager(int workspaceId);
	}
}
