using System.Threading.Tasks;
using Relativity.Sync;
using Relativity.Sync.SyncConfiguration;

namespace kCura.IntegrationPoints.RelativitySync
{
    public interface ISyncOperationsWrapper
    {
        ISyncJobFactory CreateSyncJobFactory();

        Task PrepareSyncConfigurationForResumeAsync(int workspaceId, int syncConfigurationId);

        IRelativityServices CreateRelativityServices();

        ISyncConfigurationBuilder GetSyncConfigurationBuilder(ISyncContext context);

        Task<int?> TryGetResumedSyncConfigurationIdAsync(int workspaceId, int jobHistoryId);
    }
}
