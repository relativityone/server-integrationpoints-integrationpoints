using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data
{
    public interface IRelativityObjectManagerService
    {
        IRelativityObjectManager RelativityObjectManager { get; }
    }
}
