using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync.RdoCleanup
{
    public interface ISyncRdoCleanupService
    {
        Task DeleteSyncRdosAsync(int workspaceId);
    }
}
