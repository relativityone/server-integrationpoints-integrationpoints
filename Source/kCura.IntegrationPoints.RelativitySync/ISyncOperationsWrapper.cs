using Relativity.Sync;
using Relativity.Sync.SyncConfiguration;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync
{
    public interface ISyncOperationsWrapper
    {
        ISyncJobFactory CreateSyncJobFactory();

        Task PrepareSyncConfigurationForResumeAsync(int workspaceId, int syncConfigurationId);

        IRelativityServices CreateRelativityServices();
        
        ISyncConfigurationBuilder GetSyncConfigurationBuilder(ISyncContext context);
    }
}
