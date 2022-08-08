using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Factories
{
    public interface IRelativityObjectManagerFactory
    {
        IRelativityObjectManager CreateRelativityObjectManager(int workspaceId);
        IRelativityObjectManager CreateRelativityObjectManager(int workspaceId, IServicesMgr servicesMgr);
    }
}
